using UnityEngine;
using Framework;
using Play.Hand;

namespace Play.Oil
{
    public class OilData : MonoBehaviour
    {
        public Side handSide = Side.NULL;
        private Vector3 originPos;
        public bool isUnderPot = false;//是否在锅正上方

        public Collider collider;
        
        public void Init()
        {
            Debug.Log("初始化：OilData构造");
            originPos = transform.position;
            collider = GetComponent<Collider>();
        }
        
        
    }
}