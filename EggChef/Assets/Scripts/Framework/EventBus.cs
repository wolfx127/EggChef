using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace Framework
{
    //- 类型即频道：每个 struct 类型自动隔离，不会串台，也不需要字符串 key
    //- 任意参数：需要几个参数就在 struct 里加几个字段，天然兼容各种签名
    //- 传参数：注册时得有个evt参数的函数，用这个函数注册，evt就是那个结构体，直接获取evt成员就行，广播时得new，那就在构造函数里传实参
    //- 零 GC：约束为 struct，发布时不产生堆分配
    //- Clear() / Clear<T>() 用于窗口关闭等场景下批量清理
    //使用方法：
    //先要创建对应事件（假设我们要注册一个A事件）的一个数据包TEvent
    //调用Subscribe时<>里填需要监测的TEvent，（）里填数据改变时执行的方法名
    //Publish使用时先要建立一个对应TEvent并赋值，然后就会执行A事件

    /// <summary>
    /// 基于事件类型的全局事件总线。
    /// 每种事件定义为一个 struct，字段即参数。
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> Handlers = new Dictionary<Type, Delegate>();

        //采用列表形式，防止不同的订阅同时触发

        /// <summary>
        /// 订阅事件。
        /// </summary>
        public static void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : struct, IEvent
        {
            Type key = typeof(TEvent);
            if(Handlers.TryGetValue(key,out Delegate existing))//询问TEvent有无对应的委托列表，如果没有就创建
            {
                Handlers[key] = Delegate.Combine(existing, handler);//创建了一个键值对，TEvent对应一个委托列表
            }
            else //判断列表里是否已经存在要注册的事件
            {
                //将方法加入委托列表，之后用遍历来实现里面的方法
                Handlers[key] = handler;
            }
        }

        /// <summary>
        /// 取消订阅事件。
        /// </summary>
        public static void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : struct, IEvent
        {
            Type key = typeof(TEvent);
            if (Handlers.TryGetValue(key,out Delegate existing))
            {
                Delegate updated = Delegate.Remove(existing, handler);
                // 如果没人监听了，把键值对一起删掉，释放内存
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
            if (Handlers.TryGetValue(key, out Delegate existing))//通过TEvent获取对应的委托列表
            {
                // 将基类 Delegate 安全转换为 Action<TEvent>，然后执行
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