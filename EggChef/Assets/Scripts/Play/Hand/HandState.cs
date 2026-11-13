using System;
using System.Collections.Generic;
using Framework;
using Framework.FSM;
using Play.Egg;
using Play.Pot;
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
            transitionTable.Add(new Transition(State.CatchPot,
                b => b is HandBoard hb && hb.catchObject == CatchObject.Pot));
            transitionTable.Add(new Transition(State.CatchFriedEgg,
                b => b is HandBoard hb && hb.catchObject == CatchObject.FriedEgg));
        }

        public override void Enter(HandData caller, IBoard b)
        {
            caller.itemData = null;
            Debug.Log("手返回到空闲状态");
        }

        public override void Update(HandData caller, IBoard b)
        {
            if (caller.itemGO) caller.itemGO = null;
            caller.transform.position = caller.targetPos;
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
            caller.transform.position = caller.itemGO.transform.position;
            Debug.Log("手碰到蛋框了");
            EggData egg = caller.itemGO.GetComponent<EggShell>().GetEgg(caller.side);
            if (egg != null)
            {
                caller.itemGO = egg.gameObject;
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
            caller.transform.position = caller.targetPos;
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
                    egg.rb.useGravity = false;
                    egg.handSide = caller.side;
                    egg.GetComponent<Collider>().isTrigger = true;
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
            caller.transform.position = caller.targetPos;
            float perfectSpeed = 0;
            float badSpeed = 0;
            if (caller.itemData is EggData egg)
            {
                perfectSpeed = egg.perfectSpeed;
                badSpeed = egg.badSpeed;

                if (b is HandBoard hb)
                {
                    if (egg.isPlayingAnim)
                    {
                        egg.PutAnim(hb.deltaTime);
                        return;
                    }

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
                        if(!egg.isPlayingAnim)
                        {
                            caller.itemGO.transform.position = caller.transform.position;
                        }
                    }

                    if (egg.isPerfect && hb.isPut)
                    {
                        egg.PutEgg();
                        egg.PutAnim(hb.deltaTime);
                    }
                }
            }
            else
            {
                caller.itemData = null;
                caller.itemGO = null;
            }
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

    public class HandCatchPot : BaseState<HandData>
    {
        public HandCatchPot()
        {
            transitionTable = new List<Transition>();
            transitionTable.Add(new Transition(State.CatchNothing,
                b => b is HandBoard hb && hb.catchObject == CatchObject.Nothing));
        }
        
        Vector3 originalPos = Vector3.zero;
        public override void Enter(HandData caller, IBoard b)
        {
            caller.transform.position = caller.itemGO.transform.position;
            var data = caller.itemGO.GetComponent<PotData>();
            caller.itemData = data;
            originalPos = data.originPos;
            data.handSide = caller.side;
            Debug.Log("抓起锅了");
        }

        public override void Update(HandData caller, IBoard b)
        {
            if (!caller.isMouseControlling)
            {
                caller.transform.position = originalPos;
                caller.itemGO.transform.position = originalPos;
                return;
            }

            Vector3 pos = new Vector3(
                caller.itemGO.transform.position.x,
                caller.targetPos.y,
                caller.itemGO.transform.position.z
            );
            if(pos.y < originalPos.y) pos.y = originalPos.y;
            else if(caller.itemData is PotData pot && pos.y > pot.maxHeight)
                pos.y = pot.maxHeight;
            caller.transform.position = pos;
            caller.itemGO.transform.position = caller.transform.position;
            
            if (b is HandBoard hb && caller.itemData is PotData p && !p.isFlip)
            {
                if (-hb.speed > p.perfectSpeed && -hb.speed <= p.badSpeed)
                {
                    p.PerfectFlipEnter();
                    Debug.Log($"翻得完美{-hb.speed}");
                }
                else if (-hb.speed > p.badSpeed)
                {
                    p.BadFlip();
                    Debug.Log($"翻得烂{-hb.speed}");
                }
            }
        }

        public override void Exit(HandData caller, IBoard b)
        {
            if (caller.itemData is PotData pot)
            {
                pot.ExitFromHand();
            }

            caller.itemData = null;
        }
    }

    public class CatchFriedEgg : BaseState<HandData>
    {
        public CatchFriedEgg()
        {
            transitionTable = new List<Transition>();
            transitionTable.Add(new Transition(State.CatchNothing,
                b => b is HandBoard hb && hb.catchObject == CatchObject.Nothing));
        }

        public override void Enter(HandData caller, IBoard b)
        {
            caller.transform.position = caller.itemGO.transform.position;
            caller.targetPos = caller.transform.position;

            if (caller.itemData is null)
            {
                FriedEggData egg = caller.itemGO.GetComponent<FriedEggData>();
                egg.rb.useGravity = false;
                egg.rb.isKinematic = true;
                egg.handSide = caller.side;
                egg.isFindPot = false;
                egg.EnterFromHand();
                caller.itemData = egg;
            }
        }

        public override void Update(HandData caller, IBoard b)
        {
            caller.transform.position = caller.targetPos;
            caller.itemGO.transform.position = caller.transform.position;

            if (caller.itemData is FriedEggData egg)
            {
                if (b is HandBoard hb)
                {
                    if (hb.isPut && egg.isFindPot)
                    {
                        caller.itemGO = null;
                        egg.StartFry();
                        caller.itemData = null;
                    }
                }
            }
        }

        public override void Exit(HandData caller, IBoard b)
        {
            if (caller.itemData is FriedEggData egg)
            {
                egg.ExitFromHand();
            }

            caller.itemData = null;
        }
    }

    public class CatchOil : BaseState<HandData>
    {
        public override void Enter(HandData caller, IBoard b)
        {
            throw new NotImplementedException();
        }

        public override void Update(HandData caller, IBoard b)
        {
            throw new NotImplementedException();
        }

        public override void Exit(HandData caller, IBoard b)
        {
            throw new NotImplementedException();
        }
    }
}