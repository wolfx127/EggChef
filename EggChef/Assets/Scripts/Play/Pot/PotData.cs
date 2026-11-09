using System;
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
        
        public float perfectSpeed;
        public float badSpeed;
        public float maxHeight;
        
        private float minOil;
        private float maxOil;
        public bool enableOil;

        private void Awake()
        {
            originPos = transform.position;
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
