using Play.Hand;
using UnityEngine;

namespace Framework
{
    //泛型约束，不让其他类型的结构体进来
    public interface IEvent
    {
        
    }

    public struct HandMoveByCameraEvent : IEvent
    {
        public Vector3 leftV;
        public Vector3 rightV;
    }

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
}

