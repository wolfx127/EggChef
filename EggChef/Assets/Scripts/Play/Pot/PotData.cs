using System;
using Play.Egg;
using Play.Hand;
using SO.Play;
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
        
        public float perfectSpeed;
        public float badSpeed;
        public float maxHeight;
        
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
            if(transform.GetComponentInChildren<FriedEggData>())
                Debug.Log("锅里进煎蛋了");
        }

        public void Tick(float dt)
        {
            if (friedEggData)
            {
                if(friedEggData.isFrying)
                    friedEggData.FryUpdate(dt);
                else friedEggData.FryUpdate(0);
            }
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

        public void PerfectFlip()
        {
            
        }

        public void BadFlip()
        {
            
        }
    }
}
