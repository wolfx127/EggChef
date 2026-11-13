using System;
using Framework;
using Play.Egg;
using Play.Hand;
using SO.Play;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Play.Pot
{
    //控制锅
    public class PotData : MonoBehaviour
    {
        public Side handSide = Side.NULL;
        public Vector3 originPos;
        public PotSO potSO;
        private GameObject friedEggGO;
        private FriedEggData friedEggData;
        private int friedEggLayer;
        private UniTaskLoopHandle flipHandle;
        
        public float perfectSpeed;
        public float badSpeed;
        public float maxHeight;
        public bool isFlip;
        private float eggSpeed;
        private Vector3 eggWorldPos;
        private float g;
        private float beginV;
        private float angleV;
        
        private float minOil;
        private float maxOil;
        private float curOil;

        public bool hasEgg;

        public void StartTick()
        {
            originPos = transform.position;
        }

        public void SetEgg(FriedEggData eggData)
        {
            eggData.transform.SetParent(transform,false);
            friedEggData = eggData;
            friedEggGO = eggData.gameObject;
            friedEggLayer = friedEggGO.layer;
            friedEggGO.layer = 2;
        }

        public void Tick(float dt)
        {
            
        }

        public void LatedTick(float dt)
        {
            if (isFlip && friedEggGO)
            {
                friedEggGO.transform.position = eggWorldPos;
            }
        }

        public void Init(PotSO so)
        {
            perfectSpeed = so.perfectSpeed;
            badSpeed = so.badSpeed;
            maxHeight = so.maxHeight + originPos.y;
            minOil = so.minOil;
            maxOil = so.maxOil;
            g = so.flipGravity;
            beginV = so.beginningFlipSpeed;
            angleV = so.flipAngleSpeed;
        }

        public void ExitFromHand()
        {
            handSide = Side.NULL;
            transform.position = originPos;
        }

        public void PerfectFlipEnter()
        {
            if (!friedEggGO) return;
            
            friedEggData.isFrying = false;
            //标记使得禁止互斥行为执行
            isFlip = true;
            eggSpeed = beginV;
            eggWorldPos = transform.position;
            
            flipHandle?.Stop();
            flipHandle = UniTaskMgr.RepeatEveryFixedFrame(PerfectFlipUpdate);
        }

        private void PerfectFlipUpdate()
        {
            float t = Time.fixedDeltaTime;
            //模拟重力匀加速
            eggSpeed -= t*g;
            float v = eggSpeed;
            eggWorldPos += new Vector3(0, v*t, 0);//缓存一下，因为执行时机和mono的update冲突了，
                                                  //update事先带着锅的位置算好了，所以就不是相对位置而不是世界位置了
            RollEgg(t);
            
            if(eggSpeed < 0 && friedEggGO.transform.position.y <= transform.position.y + 1f)
            {
                PerfectFlipExit();
            }
        }

        private void PerfectFlipExit()
        {
            isFlip = false;
            friedEggData.isFrying = true;
            friedEggData.FlipFace();
            friedEggGO.transform.localPosition = Vector3.zero;
            
            flipHandle?.Stop();
            flipHandle = null;
        }

        public void BadFlip()
        {
            flipHandle?.Stop();
            flipHandle = null;
            if (!friedEggGO || !friedEggData || !friedEggData.isFrying || isFlip) return;
            friedEggGO.layer = friedEggLayer;
            friedEggData.StopFry();
            friedEggGO = null;
            friedEggData = null;
        }

        private void RollEgg(float fixedDT)
        {
            if (!friedEggGO) return;
            friedEggGO.transform.Rotate(0f, 0f, angleV * fixedDT, Space.Self);
        }

        public bool CanPutEgg()
        {
            return curOil >= minOil && !friedEggGO;
        }

        public bool CanPutOil()
        {
            return !friedEggGO;
        }
    }
}
