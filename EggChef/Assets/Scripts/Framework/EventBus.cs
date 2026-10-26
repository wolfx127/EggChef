using System;
using System.Collections.Generic;

namespace Framework
{
    //- 类型即频道：每个 struct 类型自动隔离，不会串台，也不需要字符串 key
    //- 任意参数：需要几个参数就在 struct 里加几个字段，天然兼容各种签名
    //- 传参数：注册时得有个evt参数的函数，用这个函数注册，evt就是那个结构体，直接获取evt成员就行，广播时得new，那就在构造函数里传实参
    //- 零 GC：约束为 struct，发布时不产生堆分配
    //- Clear() / Clear<T>() 用于窗口关闭等场景下批量清理

    
    /// <summary>
    /// 基于事件类型的全局事件总线。
    /// 每种事件定义为一个 struct，字段即参数。
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> Handlers = new Dictionary<Type, Delegate>();

        /// <summary>
        /// 订阅事件。
        /// </summary>
        public static void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : struct, IEvent
        {
            Type key = typeof(TEvent);
            if (Handlers.TryGetValue(key, out Delegate existing))
            {
                Handlers[key] = Delegate.Combine(existing, handler);
            }
            else
            {
                Handlers[key] = handler;
            }
        }

        /// <summary>
        /// 取消订阅事件。
        /// </summary>
        public static void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : struct, IEvent
        {
            Type key = typeof(TEvent);
            if (Handlers.TryGetValue(key, out Delegate existing))
            {
                Delegate updated = Delegate.Remove(existing, handler);
                if (updated == null)
                {
                    Handlers.Remove(key);
                }
                else
                {
                    Handlers[key] = updated;
                }
            }
        }

        /// <summary>
        /// 发布事件，通知所有订阅者。
        /// </summary>
        public static void Publish<TEvent>(TEvent evt) where TEvent : struct, IEvent
        {
            Type key = typeof(TEvent);
            if (Handlers.TryGetValue(key, out Delegate existing))
            {
                ((Action<TEvent>)existing).Invoke(evt);
            }
            
            
        }

        /// <summary>
        /// 清除所有订阅（用于重置状态，如编辑器窗口关闭时）。
        /// </summary>
        public static void Clear()
        {
            Handlers.Clear();
        }

        /// <summary>
        /// 清除指定事件类型的所有订阅。
        /// </summary>
        public static void Clear<TEvent>() where TEvent : struct, IEvent
        {
            Handlers.Remove(typeof(TEvent));
        }
    }
}