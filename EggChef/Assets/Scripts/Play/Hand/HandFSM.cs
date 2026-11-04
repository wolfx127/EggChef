using Framework.FSM;
using Play.Egg;
using UnityEngine;

namespace Play.Hand
{
    public class HandFSM : BaseFSM<HandData>
    {
        public HandFSM()
        {
            b = new HandBoard();
            cur = State.CatchNothing;
            stateTable.Add(State.CatchNothing, new HandCatchNothing());
            stateTable.Add(State.CatchEggShell, new HandCatchEggShell());
            stateTable.Add(State.CatchEgg, new HandCatchEgg());
            stateTable.Add(State.CatchFriedEgg, new HandCatchFriedEgg());
            stateTable.Add(State.CatchOil, new HandCatchOil());
            stateTable.Add(State.CatchPot, new HandCatchPot());
            stateTable.Add(State.CatchBow, new HandCatchBow());
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
            if (board.catchObject == CatchObject.Nothing && itemGO)
            {
                if (itemGO.CompareTag("EggShell"))
                {
                    board.catchObject = CatchObject.EggShell;
                }
                if (itemGO.CompareTag("Egg"))
                {
                    board.catchObject = CatchObject.Egg;
                }
                if (itemGO.CompareTag("FriedEgg"))
                {
                    board.catchObject = CatchObject.FriedEgg;
                }
                if (itemGO.CompareTag("Oil"))
                {
                    board.catchObject = CatchObject.Oil;
                }
                if (itemGO.CompareTag("Pot"))
                {
                    board.catchObject = CatchObject.Pot;
                }
                if (itemGO.CompareTag("Bow"))
                {
                    board.catchObject = CatchObject.Bow;
                }
            }
            else
            {
                board.catchObject = CatchObject.Nothing;
            }
        }
    }
}
