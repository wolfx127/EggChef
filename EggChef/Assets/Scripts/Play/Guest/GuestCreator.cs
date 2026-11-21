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
        [SerializeField] private float curGapTime = 0;
        
        private float todayGuestCount = 0;

        private GuestData guestData;
        private GameObject guestGO;
        private RoadNodeMgr roadNodeMgr;
        private RoadNode startNode;

        private float roadNodeLinkDis = 10;

        private HashSet<GuestData> guestList = new();

        public void Init(PlayTimeManager timeMgr, GuestCreatorSO so, GameObject guestGO, GuestSO guestSO, 
            RoadNodeMgr roadNodeMgr)
        {
            this.timeMgr = timeMgr;
            createGapTime = so.createGapTime;
            curGapTime = createGapTime - 5f;
            this.guestGO = guestGO;
            guestData = guestGO.GetComponent<GuestData>();
            guestData.Init(guestSO);
            this.roadNodeMgr = roadNodeMgr;
            roadNodeLinkDis = so.roadNodeLinkDis;
        }

        public void StartTick()
        {
            //CreateGuest();
        }

        public void Tick(float dt)
        {
            foreach (var g in guestList)
            {
                g.Tick(dt);
            }
            
            CreateGap(dt);
        }

        public void FixedTick(float fixedDt)
        {
            foreach (var g in guestList)
            {
                g.FixedTick(fixedDt);
            }
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
            startNode = roadNodeMgr.GetRandomStartNode();
            GameObject go = Instantiate(guestGO, transform, true);
            go.transform.position = startNode.transform.position;
            SeatData seat = roadNodeMgr.GetRandomSeat();
            List<RoadNode> path = roadNodeMgr.FindPath(startNode, seat.GetComponent<RoadNode>(), roadNodeLinkDis);
            Debug.Log($"[CreateGuest] start={startNode?.name} seat={seat?.name} path={(path == null ? -1 : path.Count)}");
            go.GetComponent<GuestData>().StartTick(path, seat, this);
            
            guestList.Add(go.GetComponent<GuestData>());
            
            todayGuestCount++;
        }

        public void DestroyGuest(GuestData guest)
        {
            guestList.Remove(guest);
        }
    }
}
