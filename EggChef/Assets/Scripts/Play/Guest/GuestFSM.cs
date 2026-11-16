using Framework.FSM;
using Play.Egg;

namespace Play.Guest
{
    public class GuestFSM : BaseFSM<GuestData>
    {
        public GuestFSM(GuestData caller)
        {
            b = new GuestBoard();
            cur = State.Visit;
            this.caller = caller;

            stateTable.Add(State.Visit, new GuestVisit());
            stateTable.Add(State.Sit, new GuestSit());
            stateTable.Add(State.Wait, new GuestWait());
            stateTable.Add(State.Eat, new GuestEat());
            stateTable.Add(State.Satisfy, new GuestSatisfy());
            stateTable.Add(State.Angry, new GuestAngry());
            stateTable.Add(State.Leave, new GuestLeave());
        }

        public void SetBoard(float deltaTime, 
            bool isCatchNode, 
            bool isWait, 
            bool isSit, 
            bool isAngry, 
            FriedEggData friedEgg,
            bool isSatisfied,
            bool isLeave)
        {
            GuestBoard board = (GuestBoard)b;
            board.deltaTime = deltaTime;
            board.isCatchNode = isCatchNode;
            board.isWait = isWait;
            board.isSit = isSit;
            board.isAngry = isAngry;
            board.egg = friedEgg;
            board.satisfied = isSatisfied;
            board.isLeave = isLeave;
        }
    }
}
