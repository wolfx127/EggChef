using Framework.FSM;

namespace Play.Hand
{
    public class HandFSM : BaseFSM<Hand>
    {
        public HandFSM()
        {
            cur = State.CatchNothing;
            stateTable.Add(State.CatchNothing, new HandCatchNothing());
        }

        public void SetBoard()
        {
            
        }
    }
}
