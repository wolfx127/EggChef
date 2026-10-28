using System;
using Play.Egg;
using Play.Hand;
using SO.Play;
using UnityEngine;

namespace Play
{
    //负责初始化时: 注入mono组件，游戏对象，复用数据(取代单例)，只能有获取引用、拷贝、new的逻辑
    public class PlayInjector : MonoBehaviour
    {
        //准备注入
        PlayCamera playCamera;
        [SerializeField] private PlayCameraSO cameraSO;
        [SerializeField] private GameObject cameraGO;
        [SerializeField] private Camera cam;
        [SerializeField] private EggSO eggSO;
        [SerializeField] private GameObject eggGO;
        [SerializeField] private GameObject leftHandGO;
        [SerializeField] private GameObject rightHandGO;

        //被注入
        public PlayHandler playHandler;
        public PlayMonoController playMonoController;
        public EggShell eggShell;
        public HandData leftHand;
        public HandData rightHand;

        private void Awake()
        {
            playCamera = new PlayCamera(cameraGO, cam, cameraSO, leftHandGO, rightHandGO);
            playHandler = GetComponent<PlayHandler>();
            playMonoController = GetComponent<PlayMonoController>();
        }
        
        //onEnable能确保这些mono实例化完
        private void OnEnable()
        {
            leftHand = new HandData(leftHandGO, Side.left);
            rightHand = new HandData(rightHandGO, Side.right);
            playHandler.Init(cameraGO, playCamera, leftHand, rightHand);
            playMonoController.Init(cameraGO, playCamera, leftHand, rightHand);
            eggShell = new EggShell(eggGO, eggSO);
        }
    }
}
