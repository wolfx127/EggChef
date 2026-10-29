using UnityEngine;
using Framework;

namespace Play.Oil
{
    public class OilData
    {
        public GameObject oilGO;
        public bool isCatch = false;//是否被抓取
        public bool isPut = false;//是否按空格使用
        public bool isUnderPot = false;//是否在锅正上方
        
        public OilData(GameObject oilGO)
        {
            Debug.Log("初始化：OilData构造");
            this.oilGO = oilGO;
        }
    }
}