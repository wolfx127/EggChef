using System.Collections.Generic;

namespace Framework.FSM
{
    /// <summary>
    /// 要在子类的构造函数里填: stateTable记录的映射关系、FSM持有的caller是谁、黑板b的类型、cur初始状态
    /// 需要实现一个SetBoard()方法，填入黑板，需要 b as，因为参数列表各异就不写抽象方法了 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    //状态机FSM，黑板Board，转换表List<Transition>，状态State，四者与调用者caller注定强耦合，而正面作用是分解ifelse多层嵌套
    public abstract class BaseFSM<T> where T : class
    {
        public IBoard b;
        protected State cur;
        /// <summary>
        /// 状态映射表（枚举->类型）,构造里设置，使得fsm始终持有唯一状态实例
        /// </summary>
        protected Dictionary<State,BaseState<T>> stateTable = new Dictionary<State,BaseState<T>>();
        public T caller;
        
        public void Tick()
        {
            if (cur != State.NULL)
            {
                foreach (var transition in stateTable[cur].transitionTable)
                {
                    if (transition.condition != null && transition.condition(b))
                    {
                        stateTable[cur].Exit(caller, b);
                        stateTable[transition.to].Enter(caller, b);
                        cur = transition.to;
                        return;
                    }
                    if (transition.trigger != null && transition.trigger(b))
                    {
                        stateTable[cur].Exit(caller, b);
                        stateTable[transition.to].Enter(caller, b);
                        cur = transition.to;
                        return;
                    }
                }
                stateTable[cur].Update(caller, b);
            }
        }
    }
}
