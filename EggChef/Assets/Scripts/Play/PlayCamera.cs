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
        public float radius = 0;
        private float horizontalGap_Percent = 0;
        private float verticalGap_Percent = 0;
        
        
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

        public GameObject CatchGameObject(Vector2 screenPos, float radius)
        {
            Ray ray = cam.ScreenPointToRay(screenPos);
            GameObject go = null;
            if (Physics.Raycast(ray, out RaycastHit hit, radius))
            {
                go = hit.collider.gameObject;
            }
            return go;
        }

        public Vector3 ScreenPosToWorldPos(Vector2 screenPos, float radius)
        {
            Ray ray = cam.ScreenPointToRay(screenPos);
            return ray.GetPoint(radius);
        }

    }
}
