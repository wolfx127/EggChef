using Framework;
using Play.Egg;
using Play.Pot;
using UnityEngine;
using Object = System.Object;

namespace Play.Hand
{
    /// <summary>
    /// 主要给黑板用
    /// </summary>
    public enum CatchObject
    {
        Nothing,
        EggShell,
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
        public Transform transform => handGO.transform;
        private float speed = 1;
        private float prevPos_Y = 0;
        private float curYPos_Y = 0;
        public Object itemData = null;//手持物品的数据脚本，到时候用is判断类型
        public GameObject itemGO = null;//鼠标点到的东西
        private bool isMouseControlling = false;//被鼠标控制着
        private bool isPut = false;//是否放下道具（指把蛋、油、煎蛋这些放进指定位置，如蛋放进锅里，煎蛋放进弹弓里）
        

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
            EventBus.Subscribe<CancelCatchItemEvent>(CancelCatchItem);
        }
        

        //每帧变更状态机，手的移动不属于该状态机
        public void Tick(float dt)
        {
            UpdateSpeed(dt);
            handFSM.SetBoard(speed, itemGO, isMouseControlling, isPut);
            handFSM.Tick();
        }
        
        //其实下面的方法都和状态有关，但是他们为什么不被塞进状态机？
        //状态机里的状态都是互斥，至少针对传统fsm是这样
        //如果要强塞进状态机也可以，比如再造并行状态机，但是成本可想而知。
        //对于框架，通常只意味着 在编写时有相关的思想趋势，需要尽可能的去符合框架去写，但不一定非要这么写。
        //当其他因素的优势大于框架规范，可以试着绕过框架，图个方便，因为所有设计都以 代码能跑就行 为主要目标
        //框架服务于问题，而不是问题服务于框架
        //接下来对每个方法逐个分析
        //(其实有的放进黑板也行)
        
        //接收来自相机的事件，使得始终跟随相机
        private void MoveByCamera(HandMoveByCameraEvent evt)//它接收一个事件，也就依赖于事件的状态，但不强依赖本类状态
                                                            //所以它的行为不受本类状态控制，只是稍微控制一下状态
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
        private void MoveByMouse(HandMoveByMouseEvent evt)//同MoveByCamera
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
        private void HandFree(HandFreeEvent evt) => isMouseControlling = false;//同MoveByCamera

        private void CatchItem(CatchItemEvent evt)//这个虽然依赖事件，但其实和状态也是强依赖的，方法内频繁查改状态
                                                  //而依赖的状态中，它只是由状态推导出下一状态，而事后这些状态完全独立(指本类内)
                                                  //且其中没有哪个状态顺便执行行为
        {
            //有以下逻辑：
            //if[该手为右手]且[相机的操作手是右手]
            //    if[发来的是freeItem]
            //        if[该手没抓着东西]且[该手没握着东西]
            //            抓取()
            //        else改为[相机的操作手是左手]()
            //    else if[发来的是freezeItem]
            //        if[该手没抓着东西]且[该手没握着东西]
            //            握住()
            //        else改为[相机的操作手是左手]()
            //else if[该手为左手]且[相机的操作手是左手]
            //    if[发来的是freeItem]
            //        if[该手没抓着东西]且[该手没握着东西]
            //            抓取()
            //        else改为[相机的操作手是NULL]()
            //    else if[发来的是freezeItem]
            //        if[该手没抓着东西]且[该手没握着东西]
            //            握住()
            //        else改为[相机的操作手是NULL]()
            
            //使用离散数学的逻辑学简化逻辑，并把部分逻辑(具体抓到了什么及怎么处理)引入状态机
            //有以下逻辑：
            //if[camera的操作手不是该手] 不执行任何逻辑并退出()
            //if[该手没空]
            //    if[该手为右手]
            //        改为[camera操控的是左手]()
            //    else
            //        改为[camera操控的是NULL]()
            //else if[发来的是freeItem]或[发来的是freezeItem]
            //    该手没空()
            //    接收发来的item()
            //TODO：后期还要引入一个条件，如果这个物体已经被这个手抓住了，那么另一个手不能去抓
            
            if (evt.side.value != side) return;
            if (itemGO)
            {
                evt.side.value = side == Side.right ? Side.left : Side.NULL;
            }
            else if (evt.item.layer == LayerMask.NameToLayer("FreeItem") ||
                evt.item.layer == LayerMask.NameToLayer("FreezeItem"))
            {
                itemGO = evt.item;
            }
        }

        //把GO引用先null，是因为黑板只看GO，如果GO == null，会变成CatchNothing，于是状态机当前状态退出
        //那为什么data不在这里null? 是因为黑板不看data，保证有的状态退出时，可以执行data的逻辑，然后状态执行完记得再data置null
        private void CancelCatchItem(CancelCatchItemEvent evt)
        {
            if (evt.side != side) return;
            itemGO = null;
        }

        //获取速度
        private void UpdateSpeed(float dt)//只改状态而不驱动行为，这部分没必要进状态机。而驱动行为的逻辑确实在状态机里
                                          //所以思路倒是可以让它勉强进状态机，比如在手握鸡蛋或锅时触发
                                          //不过多个状态同时触发扔不建议进状态机，因为这意味着每个相关状态都得写一遍
                                          //简单一行计算不至于如此优化，干脆一直判断得了
        {
            curYPos_Y = handGO.transform.position.y;
            speed = (prevPos_Y - curYPos_Y)/dt;
            prevPos_Y = handGO.transform.position.y;
        }
    }
}
