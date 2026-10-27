using System.Collections.Generic;
using Framework.FSM;

namespace Play.Hand
{
    public class HandCatchNothing : BaseState<HandData>
    {
        public HandCatchNothing()
        {
            transitionTable = new List<Transition>();
        }

        public override void Enter(HandData caller, IBoard b)
        {
            
        }

        public override void Update(HandData caller, IBoard b)
        {
            
        }

        public override void Exit(HandData caller, IBoard b)
        {
            
        }
    }
}
