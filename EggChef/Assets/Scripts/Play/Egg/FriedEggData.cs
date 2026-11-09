using System;
using Play.Hand;
using Play.Pot;
using UnityEngine;

namespace Play.Egg
{
    //控制煎蛋
    public class FriedEggData : MonoBehaviour
    {
        public Collider collider;
        public Rigidbody rb;
        
        public bool isFrying = false;

        public Side handSide = Side.NULL;

        public void InitFromEgg(Vector3 pos)
        {
            collider = GetComponent<Collider>();
            rb = GetComponent<Rigidbody>();
            gameObject.SetActive(true);
            collider.isTrigger = false;
            rb.isKinematic = false;
            transform.position = pos;
        }

        public void StartFry(PotData pot)
        {
            collider.isTrigger = false;
            rb.isKinematic = false;
            transform.parent = pot.transform;
            isFrying = true;
        }

        public void Tick()
        {
            if (!isFrying) return;
        }

        public void ExitFromHand()
        {
            handSide = Side.NULL;
            collider.isTrigger = false;
            rb.useGravity = true;
            rb.isKinematic = false;
            Debug.Log("煎蛋已脱手");
        }
    }
}
