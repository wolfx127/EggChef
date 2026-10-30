using Framework.FSM;
using Play.Egg;
using UnityEngine;

namespace Play.Hand
{
    public class HandBoard : IBoard
    {
        public float speed;
        public CatchObject catchObject = CatchObject.Nothing;
        public EggState eggState = EggState.normal;
    }
}
