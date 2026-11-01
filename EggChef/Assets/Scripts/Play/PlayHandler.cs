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
            //这个ifelse用来使鼠标看起来像一次性点击的，因为鼠标是一直发射射线，如果按住时没击中手，但是按住再动鼠标移到手了也会吸附手
            //，不要让这种事出现
            //具体实现原理是加一个布尔锁，如果按下那一刻，鼠标不处于持续按住的状态(即上一帧 mouseHolding == false)，那么这时按下则会进入持续按住的状态
            //同时进一步讲，这个mouseHolding也成为了一种手势，即和leftButton.wasPressedThisFrame一样都是一种输入状态，这里表示成“鼠标按住
            //但是需要注意，鼠标客观事实上按住，不代表鼠标按下==一定按住，如果鼠标只按了一帧，但是下一帧不按了，那按住就不成立
            //所以需要在第一帧卡它一下(指wasPressedThisFrame)，不要让第一帧就执行 按住成立 的逻辑，这就又得引入一个布尔锁，表示上一帧是不是按下了
            //同时进一步讲，这个布尔锁(isMouseLeftButtonPressedFirstFrame)也成为了一个手势，这里表示成“开始进入按住状态
            //(思考：那这么弄下去，是不是还得弄个exit 表示"结束按住状态"的手势?输入系统越来越像状态机了。不过目前针对相机没必要)
            //好处是触发相机不用传输入参数了，直接一声令下，相机再不用判断输入逻辑，handler让它干什么它就干什么
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                //执行 进入按住 会触发的所有逻辑
                if(isMouseLeftButtonHoldingEnter)
                {
                    playCamera.Catching(mousePos);
                    isMouseLeftButtonHolding = true;
                }
                
                //卡一帧就是换个执行顺序的事
                isMouseLeftButtonHoldingEnter = true;
                
                //这里执行 按住 会触发的逻辑
                if (isMouseLeftButtonHolding)
                {
                    playCamera.MoveHand(mousePos);
                }
            }
            else
            {
                playCamera.CancelCatching();
                isMouseLeftButtonHoldingEnter = false;
                isMouseLeftButtonHolding = false;
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
