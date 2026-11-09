using Framework.FSM;
using Play.Egg;
using UnityEngine;

namespace Play.Hand
{
    public class HandBoard : IBoard
    {
        public float speed = 0;
        public CatchObject catchObject = CatchObject.Nothing;
        public bool isMouseControlling = false;
        public bool isPut = false;
        public float deltaTime = 0;
    }
}
