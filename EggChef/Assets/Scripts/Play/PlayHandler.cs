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
        private SideData side = new SideData();//用引用类型封装枚举，起到指针的效果，外部类拿到引用就能跨类改数值
        private bool isMouseControllingHand = false;

        private bool isMouseLeftButtonHolding = false;
        private bool isMouseLeftButtonHoldingEnter = false;

        #region UI事件

        public event Action UpdateMoney;

        #endregion
        
        private void Awake()
        {
            inputAction = new PlayInputAction();
            inputAction.Enable();
            inputAction.map.Enable();
            inputAction.map.Rotation.Enable();
            inputAction.map.Put.Enable();
        }

        private void Update()
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            var left = Mouse.current.leftButton;
            var right = Mouse.current.rightButton;

            //刚按下的一帧
            if (left.wasPressedThisFrame)
            {
                //这样catchItem()只需要执行一次，而不是只要鼠标按着就一直catching()
                CatchItem(playCamera.CatchGameObject(mousePos, playCamera.radius * 1.5f));
                isMouseLeftButtonHolding = true;
                MoveHandByMouse(playCamera.ScreenPosToWorldPos(mousePos, playCamera.radius));
            }
            //按住期间
            else if (left.isPressed)
            {
                if (isMouseLeftButtonHolding)
                    MoveHandByMouse(playCamera.ScreenPosToWorldPos(mousePos, playCamera.radius));
            }
            //松开那一瞬
            else
            {
                if (isMouseLeftButtonHolding)
                {
                    CancelMoveHandByMouse();
                    isMouseLeftButtonHolding = false;
                }
            }

            if (right.isPressed)
            {
                CancelCatchItem();
            }

            HandPutItem(inputAction.map.Put.WasPressedThisFrame());
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
        
        
        //handler的[抓取(按住鼠标左键)]输入执行，让手接下来会跟着鼠标跑(跟随动作不在这，仅表示 已跟随 状态)，前提是鼠标一开始就点到手 或 点到物体
        //接收鼠标位于屏幕的位置，制造射线一次性检测是否打到手以及物体
        private void CatchItem(GameObject hit)
        {
            if (!hit) return;
            //表示 正在控制手
            isMouseControllingHand = true;
            //表示 控制的是哪个手
            if (hit.CompareTag("LeftHand"))
                side.value = Side.left;
            else if (hit.CompareTag("RightHand"))
                side.value = Side.right;
            //如果抓的是道具
            else if (hit.gameObject.layer == LayerMask.NameToLayer("FreeItem") ||
                     hit.gameObject.layer == LayerMask.NameToLayer("FreezeItem"))
            {
                side.value = Side.right;//先通知右手，反正经过一轮广播后side会恢复成正确数值
                
                rightHand.CatchItem(side, hit);
                leftHand.CatchItem(side, hit);
                
                Debug.Log($"{side.value}Hand抓到道具[{hit.gameObject.tag}]");
            }
            else
            {
                isMouseControllingHand = false;
                side.value = Side.NULL;
            }
        }
        
        //将鼠标点中的手执行释放物体逻辑
        //原理同CatchItem，但是右键专属
        private void CancelCatchItem()
        {
            if (!isMouseControllingHand) return;
            if(side.value == Side.left) leftHand.CancelCatchItem();
            else if(side.value == Side.right) rightHand.CancelCatchItem();
            else isMouseControllingHand = false;
        }

        //在成功让 手已跟着鼠标 的前提下，执行具体的手跟随鼠标位置的逻辑
        //本质上是handler在借用camera的能力，去找屏幕之于世界的位置
        public void MoveHandByMouse(Vector3 v)
        {
            //说明左右手都没给相机回信，1可能是回了但是回的null，2两个手都没空所以没回，3干脆没回，忘记写回消息的逻辑了
            if (side.value == Side.NULL)
            {
                isMouseControllingHand = false;
                return;
            }

            if (side.value == Side.left)
            {
                leftHand.MoveByMouse(v);
            }
            else if (side.value == Side.right)
            {
                rightHand.MoveByMouse(v);
            }
        }

        //handler松开鼠标，让被控制的手恢复原样
        private void CancelMoveHandByMouse()
        {
            if(side.value == Side.NULL)
            {
                isMouseControllingHand = false;
                return;
            }
            if(side.value == Side.left)
                leftHand.HandFree();
            else if(side.value == Side.right)
                rightHand.HandFree();
            side.value = Side.NULL;
        }

        private void HandPutItem(bool isPut)
        {
            if(side.value == Side.left)
                leftHand.PutItem(isPut);
            else leftHand.PutItem(false);
            if(side.value == Side.right)
                rightHand.PutItem(isPut);
            else rightHand.PutItem(false);
        }
    }
}
