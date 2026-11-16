using System;
using System.Collections.Generic;
using Play.Decoration;
using Play.Egg;
using SO.Play;
using UnityEngine;

namespace Play.Guest
{
    public class GuestData : MonoBehaviour
    {
        public SeatData seat;
        public List<RoadNode> path;
        public int curNode = 0;
        public int nextNode = 0;
        public bool isSit = false;
        public bool isCatchNode = false;
        
        public float sightDis = 2f;
        public bool isWait = false;
        public Vector3 forward = Vector3.zero;
        public float walkSpeed = 1f;

        public bool isAngry = false;
        public float sitMaxTime = 100f;
        public FriedEggData truthEgg = null;
        public float eatTime = 0;
        public bool isSatisfied = false;
        public bool isLeave = false;
        public float bubbleTime = 0;

        public Collider collider;
        public Rigidbody rb;
        private GuestFSM FSM;
        
        public void Init(GuestSO so)
        {
            sightDis = so.sightDis;
            walkSpeed = so.walkSpeed;
        }

        public void StartTick(List<RoadNode> path, SeatData seat)
        {
            FSM = new GuestFSM(this);
            this.path = path;
            this.seat = seat;
            curNode = 0;
            if(path.Count > 1) nextNode = 1;
            
            collider = GetComponent<Collider>();
            rb = GetComponent<Rigidbody>();
        }

        public void Tick(float dt)
        {
            if (FSM == null) return;

            FSM.SetBoard(dt, isCatchNode, isWait, isSit, isAngry, truthEgg, isSatisfied, isLeave);
            FSM.Tick();
        }

        public void FixedTick(float fixedDt)
        {
            if(!isSit && !isWait)
            {
                forward = (path[nextNode].transform.position - transform.position).normalized;
                rb.MovePosition(transform.position + fixedDt * walkSpeed * forward);

                if (forward.sqrMagnitude > 0.0001f)
                {
                    Quaternion target = Quaternion.LookRotation(forward);
                    rb.MoveRotation(Quaternion.RotateTowards(
                        rb.rotation,
                        target,
                        360f * Time.deltaTime));
                }
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            if (!isSit &&
                other.gameObject.CompareTag("Seat") &&
                other.gameObject.TryGetComponent(out SeatData seatData) &&
                seatData == seat)
            {
                isSit = true;
                seatData.guest = this;
            }

            if (!isCatchNode && 
                other.gameObject.CompareTag("RoadNode") &&
                other.gameObject.TryGetComponent(out RoadNode roadNode) &&
                roadNode == path[nextNode])
            {
                isCatchNode = true;
            }

            if (other.gameObject.layer == LayerMask.NameToLayer("FreeItem"))
            {
                isAngry = true;
            }
        }
    }
}
