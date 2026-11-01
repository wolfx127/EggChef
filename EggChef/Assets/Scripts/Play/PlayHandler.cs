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

        private bool isMouseLeftButtonHolding = false;
        private bool isMouseLeftButtonHoldingEnter = false;

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
            Vector2 mousePos = Mouse.current.position.ReadValue();
            var left = Mouse.current.leftButton;

            //刚按下的一帧
            if (left.wasPressedThisFrame)
            {
                //这样catching()只需要执行一次，而不是只要鼠标按着就一直catching()
                playCamera.Catching(mousePos);
                isMouseLeftButtonHolding = true;
            }
            //按住期间
            else if (left.isPressed)
            {
                if (isMouseLeftButtonHolding)
                    playCamera.MoveHandByMouse(mousePos);
            }
            //松开那一瞬
            else
            {
                if (isMouseLeftButtonHolding)
                {
                    playCamera.CancelCatching();
                    isMouseLeftButtonHolding = false;
                }
            }
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
