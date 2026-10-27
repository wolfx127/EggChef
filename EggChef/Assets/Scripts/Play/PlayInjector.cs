using System;
using Play.Egg;
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
        [SerializeField] private EggSO eggSO;
        [SerializeField] private GameObject eggGO;
        
        //被注入
        public PlayHandler playHandler;
        public PlayMonoController playMonoController;
        public EggCase eggCase;

        private void Awake()
        {
            playCamera = new PlayCamera(cameraGO, cameraSO);
            playHandler = GetComponent<PlayHandler>();
            playMonoController = GetComponent<PlayMonoController>();
        }

        //onEnable能确保这些mono实例化完
        private void OnEnable()
        {
            playHandler.Init(cameraGO, playCamera);
            playMonoController.Init(cameraGO, playCamera);
            eggCase.Init(eggGO, eggSO);
        }
    }
}
