using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Play
{
    public class PlayHandler : MonoBehaviour
    {
        private PlayInputAction inputAction;

        [SerializeField]private GameObject cameraGO;
        private PlayCamera playCamera;

        private void Awake()
        {
            inputAction = new PlayInputAction();
            inputAction.Enable();
            inputAction.map.Enable();
            
            if(!cameraGO) Debug.Log("PlayHandler:摄像机游戏对象缺失");
        }

        private void FixedUpdate()
        {
            
        }

        private void LateUpdate()
        {
            MoveCamera(Time.deltaTime);
        }

        public void InitField(PlayCamera playCamera)
        {
            this.playCamera = playCamera;
            cameraGO.transform.rotation = Quaternion.Euler(playCamera.beginningY, playCamera.beginningX, 0f);
        }

        private void MoveCamera(float dt)
        {
            if(playCamera == null)
            {
                Debug.Log("PlayHandler:没找到摄像机脚本");
                return;
            }
            Vector2 v = inputAction.map.Rotation.ReadValue<Vector2>();

            playCamera.curX = Mathf.Clamp(
                playCamera.curX + v.x * playCamera.speed_leftNright * dt,
                playCamera.leftRange, playCamera.rightRange);
            playCamera.curY = Mathf.Clamp(
                playCamera.curY - v.y * playCamera.speed_upNdown * dt,
                playCamera.UpRange, playCamera.DownRange);

            cameraGO.transform.rotation = Quaternion.Euler(
                playCamera.beginningY + playCamera.curY,
                playCamera.beginningX + playCamera.curX,
                0f);
        }
    }
}
