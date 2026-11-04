using UnityEngine;

namespace Play.Egg
{
    public enum EggState
    {
        NULL,
        normal,
        perfect,
        bad
    }
    //控制鸡蛋
    public class EggData
    {
        public float perfectSpeed;
        public float badSpeed;
        public GameObject eggGO;
        public bool isPerfect;
        public bool isBad;

        public EggData(float perfectSpeed, float badSpeed)
        {
            Debug.Log("初始化：EggData构造");
            this.perfectSpeed = perfectSpeed;
            this.badSpeed = badSpeed;
        }

        public void TurnPerfectEgg()
        {
            if (isBad || isPerfect) return;
            isPerfect = true;
            isBad = false;
            
            //TODO:换模型
        }

        public void TurnBadEgg()
        {
            if (isBad) return;
            isBad = true;
            isPerfect = false;
            
            //TODO:换模型
        }

        public void ExitFromHand()
        {
            if (isPerfect && !isBad)
            {
                TurnBadEgg();
                eggGO.GetComponent<Rigidbody>().useGravity = true;
                eggGO.GetComponent<Collider>().isTrigger = false;
            }
        }
    }
}
