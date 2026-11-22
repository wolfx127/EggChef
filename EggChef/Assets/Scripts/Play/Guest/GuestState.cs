using System.Collections.Generic;
using Framework.FSM;
using UnityEngine;

namespace Play.Guest
{
    //通过改guest的状态来驱动状态机计算，也就是说得等下一帧写黑板才能换状态
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
            //进入排队状态
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
                    Debug.Log($"{caller.gameObject}当前节点{caller.curNode}, 下一节点{caller.nextNode}");
                }
            }
        }

        public override void Exit(GuestData caller, IBoard b)
        {
            
        }
    }
    
    public class GuestWait : BaseState<GuestData>
    {
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

    //坐等上菜
    public class GuestSit : BaseState<GuestData>
    {
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
            Debug.Log($"{caller.gameObject}客人落座");
            caller.sitTimeCounter = 0f;
        }

        public override void Update(GuestData caller, IBoard b)
        {
            if(b is GuestBoard gb)
            {
                caller.sitTimeCounter += gb.deltaTime;
                if (caller.sitTimeCounter >= caller.sitTime)
                    caller.isAngry = true;
            }
        }

        public override void Exit(GuestData caller, IBoard b)
        {
            
        }
    }
    
    //进食动画
    public class GuestEat : BaseState<GuestData>
    {
        private bool satisfied; 
        
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
            caller.eatTimeCounter = 0;
            caller.isSatisfied = false;

            if (b is GuestBoard gb)
            {
                //TODO:对比意愿煎蛋
            }
        }

        public override void Update(GuestData caller, IBoard b)
        {
            if (b is GuestBoard gb)
            {
                if(caller.eatTimeCounter < caller.eatTime)
                {
                    caller.sitTimeCounter += gb.deltaTime;
                    
                    //TODO: 对接人物动画和ui气泡动画
                }
                else
                {
                    if (satisfied)
                    {
                        caller.isSatisfied = true;
                    }
                    else
                    {
                        caller.isAngry = true;
                    }
                }
            }
        }

        public override void Exit(GuestData caller, IBoard b)
        {
        }
    }

    
    public class GuestSatisfy : BaseState<GuestData>
    {

        public GuestSatisfy()
        {
            transitionTable = new List<Transition>();
            transitionTable.Add(new Transition(State.Angry,
                b => b is GuestBoard gb && gb.isAngry));
            transitionTable.Add(new Transition(State.Leave,
                b => b is GuestBoard gb && gb.isLeave));
        }

        public override void Enter(GuestData caller, IBoard b)
        {
            caller.isSatisfied = false;
            caller.satisfiedTimeCounter = 0;
        }

        public override void Update(GuestData caller, IBoard b)
        {
            if(b is GuestBoard gb)
            {
                if (caller.satisfiedTimeCounter < caller.satisfiedTime)
                {
                    caller.sitTimeCounter += gb.deltaTime;
                    //TODO:动画
                }
                else
                {
                    //TODO:给钱
                    caller.isLeave = true;
                }
            }
        }

        public override void Exit(GuestData caller, IBoard b)
        {
        }
    }

    /// <summary>
    /// 生气状态，播完一段生气动画后转身离场。
    /// </summary>
    public class GuestAngry : BaseState<GuestData>
    {
        public GuestAngry()
        {
            transitionTable = new List<Transition>();
            transitionTable.Add(new Transition(State.Leave,
                b => b is GuestBoard gb && gb.isLeave));
        }

        public override void Enter(GuestData caller, IBoard b)
        {
            
        }

        public override void Update(GuestData caller, IBoard b)
        {
            
        }

        public override void Exit(GuestData caller, IBoard b)
        {
        }
    }
    
    public class GuestLeave : BaseState<GuestData>
    {
        public GuestLeave()
        {
            transitionTable = new List<Transition>();
        }

        public override void Enter(GuestData caller, IBoard b)
        {
            caller.rb.isKinematic = false;
            caller.collider.isTrigger = false;
            caller.curNode = caller.path.Count - 1;
            caller.nextNode = caller.curNode - 1;
        }

        public override void Update(GuestData caller, IBoard b)
        {
            if (caller.nextNode <= 0)
            {
                //TODO: 走到终点后，计时器循环，循环结束后消失
                caller.Leave();
            }

            if (b is GuestBoard board)
            {
                //TODO: 向节点移动
            }
        }

        public override void Exit(GuestData caller, IBoard b)
        {
            
        }
    }
}
