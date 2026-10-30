using System;
using Framework;
using Play.Hand;
using SO.Play;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Play
{
    /// <summary>
    /// 处理相机移动、手跟随相机移动的检测与广播
    /// </summary>
    public class PlayCamera
    {
        private PlayCameraSO cameraSO;

        private float speed_upNdown => cameraSO.speed_upNdown;
        private float speed_leftNright => cameraSO.speed_leftNright;
        private float leftRange => cameraSO.leftRange;
        private float rightRange => cameraSO.rightRange;
        private float UpRange => cameraSO.UpRange;
        private float DownRange => cameraSO.DownRange;

        private float beginningX = 0;
        private float beginningY = 0;
        private float curX = 0;
        private float curY = 0;

        private Camera cam;
        private GameObject leftHandGO;
        private GameObject rightHandGO;
        private float radius = 0;
        private float horizontalGap_Percent = 0;
        private float verticalGap_Percent = 0;
        private GameObject catchingHandGO = null;
        
        private GameObject catchingItemGO = null;
        private bool isCatchingHand = false;

        //func是C#写好的delegate，就像Action。Func允许带返回值，规定为泛型参数的最后一个
        //返回元组，元组可以看做结构体的语法糖
        private Func<(bool isMouseControlling, bool isCatchingItem, bool isHolding)> GetLeftHandState;
        private Func<(bool isMouseControlling, bool isCatchingItem, bool isHolding)> GetRightHandState;
        
        /// <summary>
        /// PlayCamera游戏摄像头初始化
        /// </summary>
        /// <param name="cameraGO">Camera游戏对象</param>
        /// <param name="cam">Camera游戏对象上的Camera组件</param>
        /// <param name="cameraSO">PlayCameraSO</param>
        /// <param name="leftHandGO">左手游戏对象</param>
        /// <param name="rightHandGO">右手游戏对象</param>
        /// <param name="GetLeftHandState">左手的获取状态方法</param>
        /// <param name="GetRightHandState">右手的获取状态方法</param>
        public PlayCamera(GameObject cameraGO, Camera cam, PlayCameraSO cameraSO, 
            GameObject leftHandGO, GameObject rightHandGO,
            Func<(bool, bool, bool)> GetLeftHandState, Func<(bool, bool, bool)> GetRightHandState)
        {
            Debug.Log("初始化：PlayCamera构造");
            cameraGO.transform.rotation = Quaternion.Euler(beginningY, beginningX, 0f);
            this.cameraSO = cameraSO;
            this.cam = cam;
            beginningX = cameraSO.beginningX;
            beginningY = cameraSO.beginningY;
            radius = cameraSO.radius;
            horizontalGap_Percent = cameraSO.horizontalGap_Percent;
            verticalGap_Percent = cameraSO.verticalGap_Percent;
            this.leftHandGO = leftHandGO;
            this.rightHandGO = rightHandGO;
            this.GetLeftHandState = GetLeftHandState;
            this.GetRightHandState = GetRightHandState;
        }
        
        //摄像机通过输入旋转，同时给手默认位置
        public void MoveCamera(GameObject cameraGO, PlayInputAction inputAction, float dt)
        {
            Vector2 v = inputAction.map.Rotation.ReadValue<Vector2>();

            curX = Mathf.Clamp(curX + v.x * speed_leftNright * dt, leftRange, rightRange);
            curY = Mathf.Clamp(curY - v.y * speed_upNdown * dt, UpRange, DownRange); 

            cameraGO.transform.rotation = Quaternion.Euler(beginningY + curY, beginningX + curX, 0f);

            //计算左右手相对相机的位置，再发布事件通知手，但手是否使用这个位置是手的事，不归相机管
            Vector3 camPos = cameraGO.transform.position;
            Vector3 leftHandPos = camPos + cam.ViewportPointToRay(new Vector3(horizontalGap_Percent, verticalGap_Percent, 0f)).direction * radius;
            Vector3 rightHandPos = camPos + cam.ViewportPointToRay(new Vector3(1f - horizontalGap_Percent, verticalGap_Percent, 0f)).direction * radius;

            EventBus.Publish(new HandMoveByCameraEvent { leftV = leftHandPos, rightV = rightHandPos });
        }

        public void Catching(Mouse mouse)
        {
            if (mouse.leftButton.isPressed)
            {
                Ray mouseRay = cam.ScreenPointToRay(mouse.position.ReadValue());
                CatchingHand(mouseRay);
                CatchingItem(mouseRay);
                HoldingItem(mouseRay);
            }
            else
            {
                CancelCatching();
            }
        }

        private void CancelCatching()
        {
            isCatchingHand = false;
            if(catchingHandGO && catchingHandGO.CompareTag("LeftHand"))
                EventBus.Publish(new HandFreeEvent{side = Side.left});
            else if(catchingHandGO && catchingHandGO.CompareTag("RightHand"))
                EventBus.Publish(new HandFreeEvent{side = Side.right});
            catchingHandGO = null;
        }

        //手的位置是鼠标在相机的位置，同时检测鼠标输入
        //如果鼠标按下，则射线检测点击的是哪个手，手由事件得知自己被选中，同时事件同时发送两个坐标，手得知自己被选中后才会拿对应的坐标
        private void CatchingHand(Ray mouseRay)
        {
            Vector3 lV = new Vector3();
            Vector3 rV = new Vector3();
            //这个if用来使鼠标看起来像一次性点击的，因为鼠标是一直发射射线，如果按住时没击中手，但是按住再动鼠标移到手了也会吸附手
            //，不要让这种事出现
            if (!isCatchingHand)
            {
                catchingHandGO = null;
                //射线检测，判断射线击中了哪些物体，out返回被点击到的物体
                if (Physics.Raycast(mouseRay, out RaycastHit hit, radius * 1.1f))
                {
                    catchingHandGO = hit.collider.gameObject;
                    //如果没检测到手&&不是手 退出
                    if (!catchingHandGO || catchingHandGO.layer != LayerMask.NameToLayer("Hand")) return;
                    isCatchingHand = true;
                }
            }

            if (catchingHandGO.CompareTag("LeftHand"))
            {
                lV = mouseRay.GetPoint(radius);
                rV = rightHandGO.transform.position;
                EventBus.Publish(new HandMoveByMouseEvent
                {
                    side = Side.left,
                    leftV = lV,
                    rightV = rV
                });
            }
            else if (catchingHandGO.CompareTag("RightHand"))
            {
                lV = leftHandGO.transform.position;
                rV = mouseRay.GetPoint(radius);
                EventBus.Publish(new HandMoveByMouseEvent
                {
                    side = Side.right,
                    leftV = lV,
                    rightV = rV
                });
            }
        }
        
        //获取一个能手里拿着的物品
        private void CatchingItem(Ray mouseRay)
        {
            //如果鼠标抓着手呢就不能抓物体，因为客观世界是这样的效果
            if (isCatchingHand) return;
            //判断物体能不能抓
            if (Physics.Raycast(mouseRay, out RaycastHit hit, radius * 2f))
            {
                //没检测到物体&&物体层级不对 直接退出
                if (!hit.collider.gameObject) return;
                var layer =  hit.collider.gameObject.layer;
                if (layer != LayerMask.NameToLayer("FreeItem"))
                    return;
            }

            isCatchingHand = true;
            if(!GetRightHandState().isMouseControlling &&
               !GetRightHandState().isCatchingItem &&
               !GetRightHandState().isHolding)
            {
                EventBus.Publish(new ItemMoveByHandEvent
                {
                    side = Side.right, itemGO = hit.collider.gameObject
                });
            }
            else if (!GetLeftHandState().isMouseControlling &&
                     !GetLeftHandState().isCatchingItem &&
                     !GetLeftHandState().isHolding)
            {
                EventBus.Publish(new ItemMoveByHandEvent
                {
                    side = Side.left, itemGO = hit.collider.gameObject
                });
            }
        }
        
        private void HoldingItem(Ray mouseRay)
        {
            //如果鼠标抓着手呢就不能抓物体，因为客观世界是这样的效果
            if (isCatchingHand) return;
            //判断物体能不能抓
            if (Physics.Raycast(mouseRay, out RaycastHit hit, radius * 2f))
            {
                //没检测到物体&&物体层级不对 直接退出
                if (!hit.collider.gameObject) return;
                var layer =  hit.collider.gameObject.layer;
                if (layer != LayerMask.NameToLayer("FreezeItem"))
                    return;
            }

            isCatchingHand = true;
            if(!GetRightHandState().isMouseControlling &&
               !GetRightHandState().isCatchingItem &&
               !GetRightHandState().isHolding)
            {
                EventBus.Publish(new HandMoveByItemEvent
                {
                    side = Side.right, itemGO = hit.collider.gameObject
                });
            }
            else if (!GetLeftHandState().isMouseControlling &&
                     !GetLeftHandState().isCatchingItem &&
                     !GetLeftHandState().isHolding)
            {
                EventBus.Publish(new HandMoveByItemEvent
                {
                    side = Side.left, itemGO = hit.collider.gameObject
                });
            }
        }
    }
}
