using System;
using Play.Hand;
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
        private HandData leftHand;
        private HandData rightHand;

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

        private void Update()
        {
            Vector3 v_left = Vector3.zero;
            Vector3 v_right = Vector3.zero;
            
//TODO:鼠标拖动改v
            
            leftHand.SetDir(v_left);
            rightHand.SetDir(v_right);
        }

        private void LateUpdate()
        {
            playCamera.MoveCamera(cameraGO, inputAction, Time.deltaTime);
        }

        public void Init(GameObject cameraGO, PlayCamera playCamera, HandData leftHand, HandData rightHand)
        {
            Debug.Log("初始化：PlayHandler");
            this.cameraGO = cameraGO;
            this.playCamera = playCamera;
            this.leftHand = leftHand;
            this.rightHand = rightHand;
        }
    }
}
