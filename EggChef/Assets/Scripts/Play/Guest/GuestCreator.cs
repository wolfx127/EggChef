using System.Collections.Generic;
using Framework;
using Play.Decoration;
using SO.Play;
using UnityEngine;

namespace Play.Guest
{
    public class GuestCreator : MonoBehaviour
    {
        private PlayTimeManager timeMgr;

        private float createGapTime = 0;
        private float curGapTime = 0;
        
        private float todayGuestCount = 0;

        private GuestData guestData;
        private GameObject guestGO;
        private RoadNodeMgr roadNodeMgr;
        private RoadNode startNode;

        public void Init(PlayTimeManager timeMgr, GuestCreatorSO so, GameObject guestGO, GuestSO guestSO, 
            RoadNodeMgr roadNodeMgr)
        {
            this.timeMgr = timeMgr;
            createGapTime = so.createGapTime;
            this.guestGO = guestGO;
            guestData = guestGO.GetComponent<GuestData>();
            guestData.Init(guestSO);
            this.roadNodeMgr = roadNodeMgr;
        }

        public void Tick(float dt)
        {
            CreateGap(dt);
        }

        private void CreateGap(float dt)
        {
            curGapTime += dt;
            if (curGapTime >= createGapTime)
            {
                curGapTime = 0;
                CreateGuest();
            }
        }

        private void CreateGuest()
        {
            GameObject go = Instantiate(guestGO, transform, true);
            SeatData seat = roadNodeMgr.GetRandomSeat();
            List<RoadNode> path = roadNodeMgr.FindPath(startNode, seat.GetComponent<RoadNode>());
            go.GetComponent<GuestData>().StartTick(path, seat);
            
            todayGuestCount++;
        }
        
    }
}
