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
        private Side isCatchingHand = Side.NULL;
        
        private GameObject catchingItemGO = null;
        private bool isLeftHandFree = true;
        private bool isRightHandFree = true;
        
        /// <summary>
        /// PlayCamera游戏摄像头初始化
        /// </summary>
        /// <param name="cameraGO">Camera游戏对象</param>
        /// <param name="cam">Camera游戏对象上的Camera组件</param>
        /// <param name="cameraSO">PlayCameraSO</param>
        /// <param name="leftHandGO">左手游戏对象</param>
        /// <param name="rightHandGO">右手游戏对象</param>
        public PlayCamera(GameObject cameraGO, Camera cam, PlayCameraSO cameraSO, GameObject leftHandGO, GameObject rightHandGO)
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

        //手的位置是鼠标在相机的位置，同时检测鼠标输入
        //如果鼠标按下，则射线检测点击的是哪个手，手由事件得知自己被选中，同时事件同时发送两个坐标，手得知自己被选中后才会拿对应的坐标
        public void CatchingHand(Mouse mouse)
        {
            if (mouse.leftButton.isPressed)
            {
                //从相机向世界打出一道射线，起到从屏幕点击到物体的效果
                Ray mouseRay = cam.ScreenPointToRay(mouse.position.ReadValue());
                Vector3 lV = new Vector3();
                Vector3 rV = new Vector3();
                if (isCatchingHand == Side.NULL)
                {
                    catchingHandGO = null;
                    //射线检测，判断射线击中了哪些物体，out返回被点击到的物体
                    if (Physics.Raycast(mouseRay, out RaycastHit hit, radius * 1.1f))
                    {
                        catchingHandGO = hit.collider.gameObject;
                    }
                }
                //如果击中手了，广播给这个手的世界位置(在视锥面上)
                if (catchingHandGO)
                {
                    if (catchingHandGO.CompareTag("LeftHand"))
                    {
                        isCatchingHand = Side.left;
                        lV = mouseRay.GetPoint(radius);
                        rV = rightHandGO.transform.position;
                        EventBus.Publish(new HandMoveByMouseEvent{
                            side = Side.left,
                            leftV = lV,
                            rightV = rV
                        });
                    }
                    else if (catchingHandGO.CompareTag("RightHand"))
                    {
                        isCatchingHand = Side.right;
                        lV = leftHandGO.transform.position;
                        rV = mouseRay.GetPoint(radius);
                        EventBus.Publish(new HandMoveByMouseEvent{
                            side = Side.right,
                            leftV = lV,
                            rightV = rV
                        });
                    }
                }
            }
            //如果鼠标松开，使手恢复默认状态，取消抓取状态
            else
            {
                isCatchingHand = Side.NULL;
                if(catchingHandGO && catchingHandGO.CompareTag("LeftHand"))
                    EventBus.Publish(new HandFreeEvent{side = Side.left});
                else if(catchingHandGO && catchingHandGO.CompareTag("RightHand"))
                    EventBus.Publish(new HandFreeEvent{side = Side.right});
                catchingHandGO = null;
            }
        }
        
        //获取一个物品
        public void CatchingItem(Mouse mouse)
        {
            if (mouse.leftButton.isPressed&&(isLeftHandFree||isRightHandFree))
            {
                Side freeSide = Side.NULL;
                freeSide = (isRightHandFree) ? Side.right : Side.left;
                
                //从相机向世界打出一道射线，起到从屏幕点击到物体的效果
                Ray mouseRay = cam.ScreenPointToRay(mouse.position.ReadValue());
                
                //TODO 这里半径两倍是怕放太远点不到，具体到时候听策划的再改吧
                if (Physics.Raycast(mouseRay, out RaycastHit hit, radius * 2f))
                {
                    catchingItemGO = hit.collider.gameObject;
                }

                if(catchingItemGO.CompareTag("Egg")||catchingHandGO.CompareTag("Oil"))
                    EventBus.Publish(new ItemMoveByHandEvent(){side=freeSide,itemGO=catchingItemGO});
            }
        }
    }
}
