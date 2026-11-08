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
    
    public struct UpdateMoneyEvent : IEvent
    {
    }
}

