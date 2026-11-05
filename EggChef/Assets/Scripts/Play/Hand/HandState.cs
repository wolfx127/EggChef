using System;
using System.Collections.Generic;
using Framework;
using Framework.FSM;
using Play.Egg;
using UnityEngine;

namespace Play.Hand
{
    public class HandCatchNothing : BaseState<HandData>
    {
        public HandCatchNothing()
        {
            transitionTable = new List<Transition>();
            //由[什么都没抓]状态 可以切换到一下状态：
            transitionTable.Add(new Transition(State.CatchEggShell,
                b => b is HandBoard hb && hb.catchObject == CatchObject.EggShell));
            transitionTable.Add(new Transition(State.CatchEgg,
                b => b is HandBoard hb && hb.catchObject == CatchObject.Egg));
        }

        public override void Enter(HandData caller, IBoard b)
        {
            Debug.Log("手返回到空闲状态");
        }

        public override void Update(HandData caller, IBoard b)
        {

        }

        public override void Exit(HandData caller, IBoard b)
        {

        }
    }

    public class HandCatchEggShell : BaseState<HandData>
    {
        public HandCatchEggShell()
        {
            transitionTable = new List<Transition>();
            transitionTable.Add(new Transition(State.CatchEgg,
                b => b is HandBoard hb && hb.catchObject == CatchObject.Egg));
            transitionTable.Add(new Transition(State.CatchNothing,
                b => b is HandBoard hb && hb.catchObject == CatchObject.Nothing));
        }

        public override void Enter(HandData caller, IBoard b)
        {
            Debug.Log("手碰到蛋框了");
            EggData egg = caller.itemGO.GetComponent<EggShell>().GetEgg();
            if (egg != null)
            {
                caller.itemGO = egg.eggGO;
                caller.itemData = egg;
            }
            else
            {
                caller.itemData = null;
                caller.itemGO = null;
            }
        }

        public override void Update(HandData caller, IBoard b)
        {

        }

        public override void Exit(HandData caller, IBoard b)
        {
            Debug.Log("手退出抓蛋框");
        }
    }

    public class HandCatchEgg : BaseState<HandData>
    {
        public HandCatchEgg()
        {
            transitionTable = new List<Transition>();
            transitionTable.Add(new Transition(State.CatchNothing,
                b => b is HandBoard hb && hb.catchObject == CatchObject.Nothing));
        }

        public override void Enter(HandData caller, IBoard b)
        {
            try
            {
                if (caller.itemData is null)
                {
                    EggData egg = caller.itemGO.GetComponent<EggData>();
                    egg.eggGO.GetComponent<Rigidbody>().useGravity = false;
                    egg.eggGO.GetComponent<Collider>().isTrigger = true;
                    caller.itemData = egg;
                }

                Debug.Log("手拿着鸡蛋");
            }
            catch(Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        public override void Update(HandData caller, IBoard b)
        {
            float perfectSpeed = 0;
            float badSpeed = 0;
            if (caller.itemData is EggData egg)
            {
                perfectSpeed = egg.perfectSpeed;
                badSpeed = egg.badSpeed;

                if (b is HandBoard hb)
                {
                    if (hb.speed >= perfectSpeed && hb.speed < badSpeed)
                    {
                        egg.TurnPerfectEgg();
                    }
                    else if (hb.speed >= badSpeed)
                    {
                        egg.TurnBadEgg();
                    }

                    if (caller.itemGO)
                    {
                        caller.itemGO.transform.position = caller.transform.position;
                    }
                    return;
                }
            }
            caller.itemData = null;
            caller.itemGO = null;
        }

        public override void Exit(HandData caller, IBoard b)
        {
            Debug.Log("手放下鸡蛋");
            //这里表示只在脱手那一刻
            if (caller.itemData is EggData egg)
            {
                egg.ExitFromHand();
            }

            caller.itemData = null;
        }
    }
}