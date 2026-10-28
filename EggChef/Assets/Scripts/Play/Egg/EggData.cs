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
        public EggState eggState = EggState.normal;

        public EggData(float perfectSpeed, float badSpeed)
        {
            Debug.Log("初始化：EggData构造");
            this.perfectSpeed = perfectSpeed;
            this.badSpeed = badSpeed;
        }
    }
}
