using System.Collections.Generic;

namespace Framework.FSM
{
    //一个state类配一个state枚举
    public enum State
    {
        NULL,
        CatchOil,
        CatchPot,
        CatchEgg,
        CatchFriedEgg,
        CatchNothing
    }

    /// <summary>
    /// 需要在构造函数里填: 转换表
    /// 使用board时，需要 b as target 目标类型。
    /// 当前状态需要执行的逻辑，可以直接操作调用者
    /// </summary>
    /// <typeparam name="T">
    /// 调用者
    /// </typeparam>
    public abstract class BaseState<T> where T : class
    {
        public State state;
        /// <summary>
        /// 转换表
        /// </summary>
        public List<Transition> transitionTable;
        //该帧刚进入此状态时执行一次
        public abstract void Enter(T caller, IBoard b);
        //该帧没进入别的状态时执行一次
        public abstract void Update(T caller, IBoard b);
        //该帧退出当前状态时执行一次
        public abstract void Exit(T caller, IBoard b);
    }
}
