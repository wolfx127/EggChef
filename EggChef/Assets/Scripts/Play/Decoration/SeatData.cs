using System;
using Play.Guest;
using UnityEngine;

namespace Play.Decoration
{
    public class SeatData : MonoBehaviour
    {
        public GuestData guest;

        [SerializeField] private GameObject plate;
        [SerializeField] private GameObject guestTriggerGO;

        private GameObject friedEgg;

        public void Init()
        {
            if (!plate)
            {
                plate = transform.Find("Plate").gameObject;
                if (!plate)
                {
                    Debug.Log($"{gameObject}座位没找到盘子");
                    return;
                }
            }

            if (!guestTriggerGO)
            {
                guestTriggerGO = transform.Find("GuestTrigger").gameObject;
                if (!guestTriggerGO)
                {
                    Debug.Log($"{gameObject}座位没找到客人触发器");
                }
            }

            plate.SetActive(false);
            guestTriggerGO.SetActive(true);
        }

        public void SetGuest(GuestData guest)
        {
            this.guest = guest;
            guestTriggerGO.SetActive(false);
            plate.SetActive(true);
            
            guest.transform.position = guestTriggerGO.transform.position;
            guest.transform.forward = (guestTriggerGO.transform.position - plate.transform.position).normalized;
            
        }

        public void SetFriedEgg(GameObject friedEgg)
        {
            if (guest && !this.friedEgg)
            {
                if (friedEgg.CompareTag("FriedEgg"))
                {
                    this.friedEgg = friedEgg;
                    Debug.Log($"{gameObject}座位接到煎蛋");
                    friedEgg.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
                    guest.SetFriedEgg(friedEgg);
                    friedEgg.transform.position = plate.transform.position + Vector3.up * 1.2f;
                    friedEgg.transform.forward = (guestTriggerGO.transform.position - plate.transform.position).normalized;
                }
            }
        }
    }
}
