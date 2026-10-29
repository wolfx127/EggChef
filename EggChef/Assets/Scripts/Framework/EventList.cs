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

    public struct OilMoveByHandEvent : IEvent
    {
        public Side side;
        public GameObject oilGO;
    }

    /// <summary>
    /// 鸡蛋被某只手拿到
    /// </summary>
    public struct EggMoveByHandEvent : IEvent
    {
        public Side side;
        public GameObject eggGO;
    }
}

