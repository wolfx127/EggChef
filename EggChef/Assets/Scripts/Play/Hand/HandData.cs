using Framework;
using Play.Egg;
using Play.Pot;
using UnityEngine;

namespace Play.Hand
{
    public enum CatchObject
    {
        Nothing,
        Egg,
        Pot,
        Oil,
        FriedEgg,
        Bow
    }

    public enum Side
    {
        NULL,
        left,
        right
    }

    public class SideData
    {
        public Side value = Side.NULL;
    }

    public class HandData
    {
        #region 基本字段

        private Side side = Side.NULL;
        private GameObject handGO;
        private CatchObject catchObject =  CatchObject.Nothing;
        private Vector3 targetPos = Vector3.zero;
        private float speed = 1;
        private float prevPos_Y = 0;
        private float curYPos_Y = 0;
        private EggData eggData = null;
        private PotData potData = null;
        private GameObject itemGO = null;
        public bool isMouseControlling = false;//被鼠标抓取着
        public bool hasItem = false;//手里抓着东西
        public bool isPut = false;//是否放下道具（指把蛋、油、煎蛋这些放进指定位置，如蛋放进锅里，煎蛋放进弹弓里）

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
            EventBus.Subscribe<CatchItemEvent>(CatchItem);
        }
        

        //每帧变更状态机，移动不属于该状态机
        public void Tick(float dt)
        {
            handFSM.SetBoard(speed, catchObject, eggData);
            handFSM.Tick();
        }

        //接收来自相机的事件，使得始终跟随相机
        private void MoveByCamera(HandMoveByCameraEvent evt)
        {
            //如果这个手没被抓住（不处于被抓住状态），就不跟随
            if (isMouseControlling) return;
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
                isMouseControlling = true;
                //挑自己对应的新位置
                if(side == Side.left) handGO.transform.position = evt.leftV;
                else if (side == Side.right) handGO.transform.position = evt.rightV;
            }
        }

        //相机通知该手状态变为 空闲 ，手随之继续跟随相机
        private void HandFree(HandFreeEvent evt) => isMouseControlling = false;

        private void CatchItem(CatchItemEvent evt)
        {
            //有以下逻辑：
            //if[该手为右手]
            //    if[发来的是freeItem]
            //        if[该手没抓着东西]且[该手没握着东西]
            //            抓取()
            //        else告诉相机去管左手()
            //    else if[发来的是freezeItem]
            //        if[该手没抓着东西]且[该手没握着东西]
            //            握住()
            //        else告诉相机去管左手()
            //else if[该手为左手]
            //    if[发来的是freeItem]
            //        if[该手没抓着东西]且[该手没握着东西]
            //            抓取()
            //        else告诉相机左手没空()
            //    else if[发来的是freezeItem]
            //        if[该手没抓着东西]且[该手没握着东西]
            //            握住()
            //        else告诉相机左手没空()
            
            //按照生成顺序为先new右手后new左手来说，注册顺序也会是先右后左，所以必定是右手先执行事件，而 左手通知相机没空 也不会冲突

            //使用离散数学的逻辑学简化逻辑，并引入状态机，并引入上述条件[先右后左]
            //if[该手有空]
            //    拿到发来的道具准备交给状态机()
            //    通知相机该手拿到了()
            //    使[该手有空]变为[该手没空]()
            //else if[该手为右手]
            //    告诉相机去管左手()
            //else告诉相机左手没空()
            if (!hasItem)
            {
                itemGO = evt.item;
                evt.side.value = side;
                hasItem = true;
            }
            else if(side == Side.right)
                evt.side.value = Side.left;
            else evt.side.value = Side.NULL;
        }
    }
}
