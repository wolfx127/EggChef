using System;
using System.Collections.Generic;
using Play.Bow;
using Play.Decoration;
using Play.Egg;
using Play.Guest;
using Play.Hand;
using Play.Oil;
using Play.Pot;
using SO.Play;
using UnityEngine;

namespace Play
{
    //负责初始化时: 注入mono组件，游戏对象，复用数据(取代单例)，只能有获取引用、拷贝、new的逻辑
    //主要是使用awake和onenable周期函数
    public class PlayInjector : MonoBehaviour
    {
        //准备注入
        PlayCamera playCamera;
        [SerializeField] private PlayCameraSO cameraSO;
        [SerializeField] private GameObject cameraGO;
        [SerializeField] private Camera cam;
        [SerializeField] private EggSO eggSO;
        [SerializeField] private GameObject eggGO;
        [SerializeField] private GameObject eggShellGO;
        [SerializeField] private GameObject leftHandGO;
        [SerializeField] private GameObject rightHandGO;
        [SerializeField] private List<GameObject> potList;
        [SerializeField] private PotSO potSO;
        [SerializeField] private GameObject oilGO;
        [SerializeField] private OilSO oilSO;
        [SerializeField] private GameObject friedEggGO;
        [SerializeField] private FriedEggSO friedEggSO;
        [SerializeField] private GameObject bowGO;
        [SerializeField] private BowSO bowSO;
        [SerializeField] private PlayTimeManagerSO playTimeManagerSO;
        [SerializeField] private GameObject guestGO;
        [SerializeField] private GuestSO guestSO;
        [SerializeField] private List<RoadNode> roadNodeList;
        [SerializeField] private List<RoadNode> startNodeList;
        [SerializeField] private List<RoadNode> seatList;
        [SerializeField] private GuestCreator guestCreator;
        [SerializeField] private GuestCreatorSO guestCreatorSO;

        //被注入
        public PlayHandler playHandler;
        public PlayMonoController playMonoController;
        public EggShell eggShell;
        public HandData leftHand;
        public HandData rightHand;
        public PlayTimeManager playTimeManager;
        public RoadNodeMgr  roadNodeMgr;

        private void Awake()
        {
            playHandler = GetComponent<PlayHandler>();
            playMonoController = GetComponent<PlayMonoController>();
            bowGO.GetComponent<Renderer>().material.color = new Color32(255, 255, 255, 0);
            
            
        }
        
        //onEnable能确保这些mono实例化完
        private void OnEnable()
        {
            rightHand = new HandData(rightHandGO, Side.right);
            leftHand = new HandData(leftHandGO, Side.left);
            playCamera = new PlayCamera(cameraGO, cam, cameraSO);
            playTimeManager = new PlayTimeManager(playTimeManagerSO);
            playHandler.Init(cameraGO, playCamera, leftHand, rightHand);
            
            foreach (var pot in potList)
                pot.GetComponent<PotData>().Init(potSO);
            
            eggShell.Init(eggGO, eggSO, eggShellGO, friedEggGO, friedEggSO);
            oilGO.GetComponent<OilData>().Init(oilSO);
            bowGO.GetComponent<BowData>().Init(bowSO);
            
            roadNodeMgr = new RoadNodeMgr(roadNodeList, startNodeList, seatList);
            
            guestCreator.Init(playTimeManager, guestCreatorSO, guestGO, guestSO, roadNodeMgr);

            foreach (var seat in seatList)
                seat.GetComponentInParent<SeatData>().Init();
            
            playMonoController.Init(cameraGO, playCamera, leftHand, rightHand, potList, guestCreator);
        }

        private void OnDestroy()
        {
            leftHand.Destroy();
            rightHand.Destroy();
        }
    }
}
