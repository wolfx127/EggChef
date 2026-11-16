using System.Collections.Generic;
using Framework.FSM;
using UnityEngine;

namespace Play.Guest
{
    public class GuestVisit : BaseState<GuestData>
    {
        public GuestVisit()
        {
            transitionTable = new();
            transitionTable.Add(new Transition(State.Wait,
                b => b is GuestBoard gb && gb.isWait));
            transitionTable.Add(new Transition(State.Sit,
                b => b is GuestBoard gb && gb.isSit));
            transitionTable.Add(new Transition(State.Angry,
                b => b is GuestBoard gb && gb.isAngry));
        }

        public override void Enter(GuestData caller, IBoard b)
        {
            
        }

        public override void Update(GuestData caller, IBoard b)
        {
            if (Physics.Raycast(caller.transform.position,
                    caller.forward, out RaycastHit hit,
                    caller.sightDis, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.CompareTag("Guest"))
                    caller.isWait = true;
            }

            if (b is GuestBoard gb)
            {
                if (gb.isCatchNode)
                {
                    caller.curNode++;
                    if (caller.curNode >= caller.path.Count - 1)
                    {
                        caller.isSit = true;
                        return;
                    }

                    caller.nextNode++;
                    caller.isCatchNode = false;
                }
            }
        }

        public override void Exit(GuestData caller, IBoard b)
        {
        }
    }
    
    public class GuestSit : BaseState<GuestData>
    {
        private float sitTimer;

        public GuestSit()
        {
            transitionTable = new List<Transition>();
            transitionTable.Add(new Transition(State.Angry,
                b => b is GuestBoard gb && gb.isAngry));
            transitionTable.Add(new Transition(State.Eat,
                b => b is GuestBoard gb && gb.egg));
        }

        public override void Enter(GuestData caller, IBoard b)
        {
            sitTimer = 0f;
        }

        public override void Update(GuestData caller, IBoard b)
        {
            GuestBoard board = (GuestBoard)b;
            sitTimer += board.deltaTime;
            if (sitTimer >= caller.sitMaxTime)
                caller.isAngry = true;
        }

        public override void Exit(GuestData caller, IBoard b)
        {
        }
    }
    
    public class GuestWait : BaseState<GuestData>
    {
        private float waitTimer;

        public GuestWait()
        {
            transitionTable = new List<Transition>();
            transitionTable.Add(new Transition(State.Visit,
                b => b is GuestBoard gb && !gb.isWait && !gb.isSit));
            transitionTable.Add(new Transition(State.Angry,
                b => b is GuestBoard gb && gb.isAngry));
        }

        public override void Enter(GuestData caller, IBoard b)
        {
            waitTimer = 0f;
        }

        public override void Update(GuestData caller, IBoard b)
        {
            if (Physics.Raycast(caller.transform.position,
                    caller.forward, out RaycastHit hit,
                    caller.sightDis, ~0, QueryTriggerInteraction.Ignore))
            {
                if (!hit.collider.CompareTag("Guest"))
                    caller.isWait = false;
            }
        }

        public override void Exit(GuestData caller, IBoard b)
        {
        }
    }

    public class GuestEat : BaseState<GuestData>
    {
        private float eatTimer;

        public GuestEat()
        {
            transitionTable = new List<Transition>();
            transitionTable.Add(new Transition(State.Angry,
                b => b is GuestBoard gb && gb.isAngry));
            transitionTable.Add(new Transition(State.Satisfy,
                b => b is GuestBoard gb && gb.satisfied));
        }

        public override void Enter(GuestData caller, IBoard b)
        {
            eatTimer = 0f;
            caller.isSatisfied = false;
        }

        public override void Update(GuestData caller, IBoard b)
        {
            if (caller.isAngry) return;

            GuestBoard board = (GuestBoard)b;
            eatTimer += board.deltaTime;
            if (eatTimer >= caller.eatTime)
                caller.isSatisfied = true;
        }

        public override void Exit(GuestData caller, IBoard b)
        {
        }
    }

    /// <summary>
    /// 吃饱后的表演状态。这里可以按时间切多段动作，别把延迟塞进 Eat.Exit。
    /// </summary>
    public class GuestSatisfy : BaseState<GuestData>
    {
        private float resultTimer;
        private bool phase1;
        private bool phase2;

        public GuestSatisfy()
        {
            transitionTable = new List<Transition>();
            transitionTable.Add(new Transition(State.Leave,
                b => b is GuestBoard gb && gb.isLeave));
        }

        public override void Enter(GuestData caller, IBoard b)
        {
            resultTimer = 0f;
            phase1 = false;
            phase2 = false;
        }

        public override void Update(GuestData caller, IBoard b)
        {
            GuestBoard board = (GuestBoard)b;
            resultTimer += board.deltaTime;

            // 多段庆祝动作：可以换成动画事件或 Timeline 回调
            if (!phase1 && resultTimer >= 0.35f)
            {
                phase1 = true;
                // TODO: 放下筷子 / 动作 1
            }

            if (!phase2 && resultTimer >= 1.0f)
            {
                phase2 = true;
                // TODO: 伸懒腰 / 满意表情 / 动作 2
            }

            if (resultTimer >= caller.bubbleTime)
                caller.isLeave = true;
        }

        public override void Exit(GuestData caller, IBoard b)
        {
        }
    }

    /// <summary>
    /// 生气状态，播完一段生气表现后离场。
    /// </summary>
    public class GuestAngry : BaseState<GuestData>
    {
        private float resultTimer;

        public GuestAngry()
        {
            transitionTable = new List<Transition>();
            transitionTable.Add(new Transition(State.Leave,
                b => b is GuestBoard gb && gb.isLeave));
        }

        public override void Enter(GuestData caller, IBoard b)
        {
            resultTimer = 0f;
        }

        public override void Update(GuestData caller, IBoard b)
        {
            GuestBoard board = (GuestBoard)b;
            resultTimer += board.deltaTime;

            // TODO: 生气动画 / 摔门 / 气泡
            if (resultTimer >= caller.bubbleTime)
                caller.isLeave = true;
        }

        public override void Exit(GuestData caller, IBoard b)
        {
        }
    }

    /// <summary>
    /// 离场。当前直接延迟销毁，之后可以改成沿出门路径走。
    /// </summary>
    public class GuestLeave : BaseState<GuestData>
    {
        public GuestLeave()
        {
            transitionTable = new List<Transition>();
        }

        public override void Enter(GuestData caller, IBoard b)
        {
            caller.curNode = caller.path.Count - 1;
            caller.nextNode = caller.curNode - 1;
        }

        public override void Update(GuestData caller, IBoard b)
        {
            if (caller.nextNode <= 0)
            {
                //TODO: 计时器循环，循环结束后消失
            }

            if (b is GuestBoard board)
            {
                //TODO: 原路返回
            }
        }

        public override void Exit(GuestData caller, IBoard b)
        {
        }
    }
}
