using System;
using Framework;
using Play.Hand;
using SO.Play;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Play
{
    /// <summary>
    /// 处理相机移动、手跟随相机移动的检测与广播，因为手和输入都需要依赖相机确定位置，所以方法就写在这了
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
        
        //摄像机自己的移动逻辑
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

        //handler的[抓取(按住鼠标左键)]输入执行，让手接下来会跟着鼠标跑(跟随动作不在这，仅表示 已跟随 状态)，前提是鼠标一开始就点到手 或 点到物体
        //接收鼠标位于屏幕的位置，制造射线一次性检测是否打到手以及物体
        public void CatchItem(Vector2 mousePos)
        {
            Ray ray = cam.ScreenPointToRay(mousePos);
            if (!Physics.Raycast(ray, out RaycastHit hit, radius * 2f))
            {
                side.value = Side.NULL;
                isMouseControllingHand = false;
                return;
            }
            //表示 正在控制手
            isMouseControllingHand = true;
            //表示 控制的是哪个手
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
        
        //将鼠标点中的手执行释放物体逻辑
        //原理同CatchItem，但是右键专属
        public void CancelCatchItem(Vector2 mousePos)
        {
            if (!isMouseControllingHand) return;
            EventBus.Publish(new CancelCatchItemEvent{side = side.value});
            side.value = Side.NULL;
        }

        //在成功让 手已跟着鼠标 的前提下，执行具体的手跟随鼠标位置的逻辑
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

        //handler松开鼠标，让被控制的手恢复原样
        public void CancelMoveHandByMouse()
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
