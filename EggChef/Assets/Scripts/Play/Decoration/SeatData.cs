using System;
using Play.Guest;
using UnityEngine;

namespace Play.Decoration
{
    public class SeatData : MonoBehaviour
    {
        public GuestData guest;

        public GameObject plate;
        public GameObject chair;

        private GameObject friedEgg;
        
        private Collider guestCollider;
        private Collider plateCollider;

        public void Init()
        {
            if (!plate)
            {
                plate = transform.GetChild(0).gameObject;
                if (!plate)
                {
                    Debug.Log("座位没找到盘子");
                    return;
                }
            }
            plate.SetActive(false);
            
            plateCollider = plate.GetComponent<Collider>();
            guestCollider = GetComponent<Collider>();
            guestCollider.isTrigger = true;
            plateCollider.isTrigger = true;
        }

        public void SetGuest(GuestData guest)
        {
            this.guest = guest;
            guestCollider.enabled = false;
            plate.SetActive(true);
            
            //TODO:摆正客人位置
            
            
        }

        public void SetFriedEgg(GameObject friedEgg)
        {
            this.friedEgg = friedEgg;
            guest.SetFriedEgg(friedEgg);
            
        }
    }
}
