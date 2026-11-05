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
    public class EggData : MonoBehaviour
    {
        public float perfectSpeed;
        public float badSpeed;
        public GameObject eggGO;
        public bool isPerfect;
        public bool isBad;
        private EggShell eggShell;

        public void Init(float perfectSpeed, float badSpeed, EggShell eggShell)
        {
            Debug.Log("初始化：EggData构造");
            this.perfectSpeed = perfectSpeed;
            this.badSpeed = badSpeed;
            this.eggShell = eggShell;
            
            isPerfect = false;
            isBad = false;
        }

        public void TurnPerfectEgg()
        {
            if (isBad || isPerfect) return;
            isPerfect = true;
            isBad = false;
            
            //TODO:换模型
            
            Debug.Log("敲得完美");
        }

        public void TurnBadEgg()
        {
            if (isBad) return;
            isBad = true;
            isPerfect = false;
            
            //TODO:换模型
            
            Debug.Log("敲得烂");
        }

        public void ExitFromHand()
        {
            if (isPerfect)
            {
                TurnBadEgg();
            }
            if (isBad)
            {
                eggGO.GetComponent<Rigidbody>().useGravity = true;
                eggGO.GetComponent<Collider>().isTrigger = false;
            }
            else
            {
                eggShell.ReturnEgg(this);
            }
            Debug.Log("鸡蛋已脱手");
        }
    }
}
