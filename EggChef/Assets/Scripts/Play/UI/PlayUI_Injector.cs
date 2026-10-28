using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Play.UI
{
    //针对ui的注入，不同于PlayInjector，该脚本只负责mvp三层
    public class PlayUI_Injector : MonoBehaviour
    {
        [SerializeField]private PlayHandler playHandler;
        private IPlayUI_P playUI_P;
        private IPlayUI_V playUI_V;
        
        private VisualElement root;

        private void Awake()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
        }

        private void OnEnable()
        {
            playUI_V = new PlayUI_V();
            playUI_P = new PlayUI_P(playUI_V, playHandler);
            playUI_V.Init(playUI_P, root);
            playUI_P.Subscribe();
        }

        private void OnDisable()
        {
            playUI_P.Unsubscribe();
        }
    }
}
