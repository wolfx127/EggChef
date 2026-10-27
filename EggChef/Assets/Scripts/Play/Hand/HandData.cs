using Play.Egg;
using UnityEngine;

namespace Play.Hand
{
    public enum CatchObject
    {
        nothing,
        egg,
        pot,
        oil,
        FriedEgg
    }

    public class HandData
    {
        #region 基本字段

        private GameObject HandGO;
        private CatchObject catchObject =  CatchObject.nothing;
        private Vector3 dir = Vector3.zero;    
        private float speed;
        private float prevPos_Y = 0;
        private float curYPos_Y = 0;
        private EggData EggData = null;

        #endregion

        private Rigidbody rb;
        private HandFSM handFSM =  new HandFSM();

        public HandData(GameObject handGO)
        {
            this.HandGO = handGO;
            rb = handGO.GetComponent<Rigidbody>();
        }

        public void Tick(float dt)
        {
            handFSM.SetBoard(speed, catchObject, EggData);
            handFSM.Tick();
        }

        public void Move(float fixedDt)
        {
            curYPos_Y = HandGO.transform.position.y;
            rb.MovePosition(dir * fixedDt + HandGO.transform.position);
            speed = prevPos_Y - curYPos_Y;
            prevPos_Y = curYPos_Y;
        }

        public void SetDir(Vector3 v) => dir = v;
    }
}
