using System;

namespace Framework.FSM
{
    /// <summary>
    /// 单个转换信息，to下一个状态，condition游戏中程序员认为的持续状态，trigger游戏中程序员认为的单帧即可触发的瞬时状态
    /// fsm每帧用它读黑板，判断黑板符合哪个状态，然后判断下一个状态
    /// </summary>
    public class Transition
    {
        public State to;
        public Func<IBoard, bool> condition;
        public Func<IBoard, bool> trigger;
        public Transition(State to, Func<IBoard, bool> condition, Func<IBoard, bool> trigger = null)
        {
            this.to = to;
            this.condition = condition;
            this.trigger = trigger;
        }
    }
}
