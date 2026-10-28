using Framework;
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

    public enum Side
    {
        NULL,
        left,
        right
    }

    public class HandData
    {
        #region 基本字段

        private Side side = Side.left;
        private GameObject handGO;
        private CatchObject catchObject =  CatchObject.nothing;
        private Vector3 targetPos = Vector3.zero;
        private float speed = 1;
        private float prevPos_Y = 0;
        private float curYPos_Y = 0;
        private EggData eggData = null;
        private PotData potData = null;
        public bool isCatch = false;
        public bool isPut = false;

        #endregion

        private Rigidbody rb;
        private HandFSM handFSM = new HandFSM();

        public HandData(GameObject handGO, Side side)
        {
            Debug.Log("初始化：HandData构造");
            this.handGO = handGO;
            rb = handGO.GetComponent<Rigidbody>();
            this.side = side;
            EventBus.Subscribe<HandMoveByCameraEvent>(MoveByCamera);
            EventBus.Subscribe<HandMoveByMouseEvent>(MoveByMouse);
            EventBus.Subscribe<HandFreeEvent>(HandFree);
        }

        public void Tick(float dt)
        {
            handFSM.SetBoard(speed, catchObject, eggData);
            handFSM.Tick();
        }

        private void MoveByCamera(HandMoveByCameraEvent evt)
        {
            if (isCatch) return;
            if (side == Side.left)
            {
                handGO.transform.position = evt.leftV;
            }

            if (side == Side.right)
            {
                handGO.transform.position = evt.rightV;
            }
        }

        private void MoveByMouse(HandMoveByMouseEvent evt)
        {
            if (evt.side == side)
            {
                isCatch = true;
                if(side == Side.left) handGO.transform.position = evt.leftV;
                else if (side == Side.right) handGO.transform.position = evt.rightV;
            }
        }
        
        private void HandFree(HandFreeEvent evt) => isCatch = false;
    }
}
