using Framework.FSM;
using Play.Egg;
using Play.Pot;
using UnityEngine;

namespace Play.Hand
{
    public class HandFSM : BaseFSM<HandData>
    {
        public HandFSM(HandData caller)
        {
            b = new HandBoard();
            cur = State.CatchNothing;
            this.caller = caller;
            stateTable.Add(State.CatchNothing, new HandCatchNothing());
            stateTable.Add(State.CatchEggShell, new HandCatchEggShell());
            stateTable.Add(State.CatchEgg, new HandCatchEgg());
            stateTable.Add(State.CatchPot, new HandCatchPot());
            stateTable.Add(State.CatchFriedEgg, new CatchFriedEgg());
        }

        public void SetBoard(float speed, 
            GameObject itemGO,
            bool isMouseControlling,
            bool isPut,
            float deltaTime)
        {
            HandBoard board = (HandBoard)b;
            board.isMouseControlling = isMouseControlling;
            board.isPut = isPut;
            board.speed = speed;
            board.deltaTime = deltaTime;
            if(!itemGO) board.catchObject = CatchObject.Nothing;
            else switch (itemGO.tag)
            {
                case "EggShell": board.catchObject = CatchObject.EggShell; break;
                case "Egg":
                    EggData egg = itemGO.GetComponent<EggData>();
                    if (egg.handSide == Side.NULL || egg.handSide == caller.side)
                        board.catchObject = CatchObject.Egg;
                    else
                        board.catchObject = CatchObject.Nothing;
                    break;
                case "FriedEgg": board.catchObject = CatchObject.FriedEgg; break;
                case "Oil":      board.catchObject = CatchObject.Oil; break;
                case "Pot":
                    PotData pot = itemGO.GetComponent<PotData>();
                    if (pot.handSide == Side.NULL || pot.handSide == caller.side)
                        board.catchObject = CatchObject.Pot;
                    else
                        board.catchObject = CatchObject.Nothing;
                    break;
                case "Bow":      board.catchObject = CatchObject.Bow; break;
                default:         board.catchObject = CatchObject.Nothing; break;
            }
        }
    }
}
