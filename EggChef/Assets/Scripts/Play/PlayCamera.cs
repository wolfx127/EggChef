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
        private float radius = 0;
        private float horizontalGap_Percent = 0;
        private float verticalGap_Percent = 0;
        
        private SideData side = new SideData();//用引用类型封装枚举，起到指针的效果，外部类拿到引用就能跨类改数值
        private bool isMouseControllingHand = false;
        
        /// <summary>
        /// PlayCamera游戏摄像头初始化
        /// </summary>
        /// <param name="cameraGO">Camera游戏对象</param>
        /// <param name="cam">Camera游戏对象上的Camera组件</param>
        /// <param name="cameraSO">PlayCameraSO</param>
        public PlayCamera(GameObject cameraGO, Camera cam, PlayCameraSO cameraSO)
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

        //接收鼠标位于屏幕的位置，制造射线一次性检测是否打到手以及物体
        public void Catching(Vector2 mousePos)
        {
            Ray ray = cam.ScreenPointToRay(mousePos);
            if (!Physics.Raycast(ray, out RaycastHit hit, radius * 2f))
            {
                side.value = Side.NULL;
                isMouseControllingHand = false;
                return;
            }
            isMouseControllingHand = true;
            if (hit.collider.CompareTag("LeftHand"))
                side.value = Side.left;
            else if (hit.collider.CompareTag("RightHand"))
                side.value = Side.right;
            else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("FreeItem") ||
                     hit.collider.gameObject.layer == LayerMask.NameToLayer("FreezeItem"))
            {
                side.value = Side.right;//先通知右手，反正经过一轮广播后side会恢复成正确数值
                EventBus.Publish(new CatchItemEvent { side = this.side, item = hit.collider.gameObject });
                Debug.Log($"{side.value}Hand抓到道具[{hit.collider.gameObject.tag}]");
            }
            else isMouseControllingHand = false;
        }
        
        //本质上是handler在借用camera的能力，去找屏幕之于世界的位置
        public void MoveHandByMouse(Vector2 mousePos)
        {
            //说明左右手都没给相机回信，1可能是回了但是回的null，2两个手都没空所以没回，3干脆没回，忘记写回消息的逻辑了
            if (side.value == Side.NULL) return;
            
            Ray ray = cam.ScreenPointToRay(mousePos);
            Vector3 lV = ray.GetPoint(radius);
            Vector3 rV = ray.GetPoint(radius);
            
            EventBus.Publish(new HandMoveByMouseEvent{side = side.value, leftV = lV, rightV = rV});
        }

        public void CancelCatching()
        {
            if(side.value == Side.NULL)
            {
                isMouseControllingHand = false;
                return;
            }
            if(side.value == Side.left)
                EventBus.Publish(new HandFreeEvent{side = Side.left});
            else if(side.value == Side.right)
                EventBus.Publish(new HandFreeEvent{side = Side.right});
            side.value = Side.NULL;
        }
    }
}
