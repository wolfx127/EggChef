using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Play
{
    //负责输入输出相关，包括键鼠，UI,除此之外的new和拷贝不被允许
    public class PlayHandler : MonoBehaviour
    {
        private PlayInputAction inputAction;
        private GameObject cameraGO;
        private PlayCamera playCamera;

        #region UI事件

        public event Action RefreshUIEvent;

        #endregion
        
        private void Awake()
        {
            inputAction = new PlayInputAction();
            inputAction.Enable();
            inputAction.map.Enable();
            inputAction.map.Rotation.Enable();
        }

        private void LateUpdate()
        {
            playCamera.MoveCamera(cameraGO, inputAction, Time.deltaTime);
        }

        public void Init(GameObject cameraGO, PlayCamera playCamera)
        {
            this.cameraGO = cameraGO;
            this.playCamera = playCamera;
        }
    }
}
