using System;
using SO.Play;
using UnityEngine;

namespace Play
{
    public class PlayInjector : MonoBehaviour
    {
        //准备注入
        PlayCamera playCamera;
        [SerializeField] private PlayCameraSO cameraSO;
        
        //被注入
        public PlayHandler playHandler;

        private void OnEnable()
        {
            playCamera = new PlayCamera(cameraSO);
            playHandler.InitField(playCamera);
            
        }
    }
}
