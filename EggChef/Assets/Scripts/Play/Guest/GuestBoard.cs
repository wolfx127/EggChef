using Framework.FSM;
using Play.Egg;

namespace Play.Guest
{
    public class GuestBoard : IBoard
    {
        public float deltaTime;
        public bool isCatchNode;
        public bool isSit;
        public bool isWait;
        public bool isAngry;
        public FriedEggData egg = null;
        public bool satisfied;
        public bool isLeave;
    }
}
