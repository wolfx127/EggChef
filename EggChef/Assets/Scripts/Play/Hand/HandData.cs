using Framework;
using Play.Egg;
using Play.Pot;
using UnityEngine;

namespace Play.Hand
{
    public enum CatchObject
    {
        nothing,
        egg,
        pot,
        oil,
        FriedEgg
    }

    public enum Side
    {
        NULL,
        left,
        right
    }

    public class HandData
    {
        #region 基本字段

        private Side side = Side.NULL;
        private GameObject handGO;
        private CatchObject catchObject =  CatchObject.nothing;
        private Vector3 targetPos = Vector3.zero;
        private float speed = 1;
        private float prevPos_Y = 0;
        private float curYPos_Y = 0;
        private EggData eggData = null;
        private PotData potData = null;
        private GameObject itemGO = null;
        public bool isCatched = false;//被鼠标抓取
        public bool isCatching = false;//该手获取物品
        public bool isPut = false;

        #endregion

        private Rigidbody rb;
        private HandFSM handFSM = new HandFSM();

        public HandData(GameObject handGO, Side side)
        {
            Debug.Log("初始化：HandData构造");
            this.handGO = handGO;
            rb = handGO.GetComponent<Rigidbody>();
            this.side = side;
            EventBus.Subscribe<HandMoveByCameraEvent>(MoveByCamera);
            EventBus.Subscribe<HandMoveByMouseEvent>(MoveByMouse);
            EventBus.Subscribe<HandFreeEvent>(HandFree);
            EventBus.Subscribe<OilMoveByHandEvent>(MoveByHand);
        }
        

        //每帧变更状态机，移动不属于该状态机
        public void Tick(float dt)
        {
            handFSM.SetBoard(speed, catchObject, eggData);
            handFSM.Tick();

            if (isCatching)
            {
                itemGO.transform.position = handGO.transform.position;
            }
        }

        //接收来自相机的事件，使得始终跟随相机
        private void MoveByCamera(HandMoveByCameraEvent evt)
        {
            //如果这个手没被抓住（不处于被抓住状态），就不跟随
            if (isCatched) return;
            if (side == Side.left)
            {
                handGO.transform.position = evt.leftV;
            }

            if (side == Side.right)
            {
                handGO.transform.position = evt.rightV;
            }
        }

        //接收来自相机的事件，同时相机发来一个 抓住 的状态变更操作，以及变更后的新位置
        private void MoveByMouse(HandMoveByMouseEvent evt)
        {
            //看自己是不是被抓住的那只手
            if (evt.side == side)
            {
                //状态变更
                isCatched = true;
                //挑自己对应的新位置
                if(side == Side.left) handGO.transform.position = evt.leftV;
                else if (side == Side.right) handGO.transform.position = evt.rightV;
            }
        }
        
        private void MoveByHand(OilMoveByHandEvent evt)
        {
            if (evt.side == side)
            {
                isCatching = true;
                itemGO = evt.oilGO;
            }
        }
        
        //相机通知该手状态变为 空闲 ，手随之继续跟随相机
        private void HandFree(HandFreeEvent evt) => isCatched = false;
    }
}
