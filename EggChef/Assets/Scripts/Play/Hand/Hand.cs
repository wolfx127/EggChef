using UnityEngine;

namespace Play.Hand
{
    public class Hand
    {
        #region 基本字段

        private GameObject HandGO;
        private bool isCatchPot = false;

        #endregion
        
        private HandFSM handFSM =  new HandFSM();

        public Hand(GameObject handGO)
        {
            this.HandGO = handGO;
        }

        public void Tick(float dt)
        {
            handFSM.SetBoard();
            handFSM.Tick();
        }

        public void Move(float fixedDt)
        {
            
        }
    }
}
