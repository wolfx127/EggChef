using Framework.FSM;
using Play.Egg;
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
        }

        public void SetBoard(float speed, 
            GameObject itemGO,
            bool isMouseControlling,
            bool isPut)
        {
            HandBoard board = (HandBoard)b;
            board.isMouseControlling = isMouseControlling;
            board.isPut = isPut;
            board.speed = speed;
            if(!itemGO) board.catchObject = CatchObject.Nothing;
            else switch (itemGO.tag)
            {
                case "EggShell": board.catchObject = CatchObject.EggShell; break;
                case "Egg":      board.catchObject = CatchObject.Egg; break;
                case "FriedEgg": board.catchObject = CatchObject.FriedEgg; break;
                case "Oil":      board.catchObject = CatchObject.Oil; break;
                case "Pot":      board.catchObject = CatchObject.Pot; break;
                case "Bow":      board.catchObject = CatchObject.Bow; break;
                default:         board.catchObject = CatchObject.Nothing; break;
            }
        }
    }
}
