using System;
using Play.Hand;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Play
{
    //负责输入输出相关，包括键鼠，UI,除此之外的new和拷贝不被允许
    //特殊的monoController，只要依赖输入和UI交互的，都要从monoController移到这里
    public class PlayHandler : MonoBehaviour
    {
        private PlayInputAction inputAction;
        private GameObject cameraGO;
        private PlayCamera playCamera;
        private HandData leftHand;
        private HandData rightHand;
        private GameObject catchObject;
        #region UI事件

        public event Action RefreshUIEvent;

        #endregion
        
        private void Awake()
        {
            inputAction = new PlayInputAction();
            inputAction.Enable();
            inputAction.map.Enable();
            inputAction.map.Rotation.Enable();
            inputAction.map.PickUpItem.started += CreateItem;
        }


        private void Update()
        {
            playCamera.CatchingHand(Mouse.current);
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
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("PickcableItems"))
            {
                catchObject=this.gameObject;
            }
        }
        private void OnTriggerExit(Collider other)
        {
            catchObject = null;
        }
        private void CreateItem(InputAction.CallbackContext obj)
        {
            if (catchObject != null)
            {
                Instantiate(catchObject);
            }
        }
    }
}
