using Play.Egg;
using Play.Hand;
using UnityEngine;
using UnityEngine.UIElements;

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

    public struct UpdateMoneyEvent : IEvent
    {
    }
}

