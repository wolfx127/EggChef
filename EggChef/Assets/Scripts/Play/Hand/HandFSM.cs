using Framework.FSM;
using Play.Egg;

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
            CatchObject catchObject,
            EggData eggData)
        {
            HandBoard board = (HandBoard)b;
            board.speed = speed;
            board.catchObject = catchObject;
            board.eggState = EggState.NULL;
            if(eggData != null) board.eggState = eggData.eggState;
        }
    }
}
