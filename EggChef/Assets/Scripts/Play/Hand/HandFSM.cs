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
        }

        public void SetBoard(float speed, 
            GameObject itemGO,
            bool isMouseControlling)
        {
            HandBoard board = (HandBoard)b;
        }
    }
}
