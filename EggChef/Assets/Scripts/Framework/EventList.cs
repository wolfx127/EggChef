using Play.Egg;
using Play.Hand;
using UnityEngine;

namespace Framework
{
    //泛型约束，不让其他类型的结构体进来
    public interface IEvent
    {
        
    }

    /// <summary>
    /// 手跟随相机移动
    /// </summary>
    public struct HandMoveByCameraEvent : IEvent
    {
        public Vector3 leftV;
        public Vector3 rightV;
    }

    /// <summary>
    /// 手被鼠标拖动
    /// </summary>
    public struct HandMoveByMouseEvent : IEvent
    {
        public Side side;
        public Vector3 leftV;
        public Vector3 rightV;
    }

    public struct HandFreeEvent : IEvent
    {
        public Side side;
    }

    public struct CatchItemEvent : IEvent
    {
        public SideData side;
        /// <summary>
        /// 用作查看tag和layer等身份证明
        /// </summary>
        public GameObject item;
    }

    public struct CancelCatchItemEvent : IEvent
    {
        public Side side;
    }

    /// <summary>
    /// 通过发布该事件返回一个鸡蛋，靠引用返回值
    /// </summary>
    public struct ReturnEggEvent : IEvent
    {
        public EggData eggData;
    }
}

