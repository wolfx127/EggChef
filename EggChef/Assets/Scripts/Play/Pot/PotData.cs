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
        
        private float minOil;
        private float maxOil;
        public bool enableOil;

        public void StartTick()
        {
            originPos = transform.position;
        }

        public void SetEgg(FriedEggData eggData)
        {
            friedEggData = eggData;
            friedEggGO = eggData.gameObject;
            friedEggLayer = friedEggGO.layer;
            friedEggGO.layer = 2;
        }

        public void Tick(float dt)
        {
            
        }

        public void Init(PotSO so)
        {
            perfectSpeed = so.perfectSpeed;
            badSpeed = so.badSpeed;
            maxHeight = so.maxHeight + originPos.y;
            minOil = so.minOil;
            maxOil = so.maxOil;
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
            
            flipHandle?.Stop();
            flipHandle = UniTaskMgr.RepeatEveryFixedFrame(PerfectFlipUpdate);
        }

        private void PerfectFlipUpdate()
        {
            float t = Time.fixedDeltaTime;
            float v = perfectSpeed * 1.2f;
            float g = 9.8f;
            //模拟重力匀加速
            float x = v * t - 0.5f * g * t * t;
            friedEggGO.transform.position += new Vector3(0, x, 0);
            RollEgg(t);
            
            if(friedEggGO.transform.position.y <=transform.position.y)
            {
                PerfectFlipExit();
            }
        }

        private void PerfectFlipExit()
        {
            isFlip = false;
            friedEggData.isFrying = true;
            friedEggData.FlipFace();
        }

        public void BadFlip()
        {
            if (!friedEggGO || !friedEggData || !friedEggData.isFrying || isFlip) return;
            friedEggGO.layer = friedEggLayer;
            friedEggData.StopFry();
            friedEggGO = null;
            friedEggData = null;
        }

        private void RollEgg(float fixedDT)
        {
            if (!friedEggGO) return;
            float angleSpeed = 10f;
            friedEggGO.transform.Rotate(0f, angleSpeed * fixedDT, 0f, Space.Self);
        }
    }
}
