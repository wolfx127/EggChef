using System;
using Cysharp.Threading.Tasks;
using Framework;
using Overall;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using YooAsset;

namespace StartToPlay
{
    public class InitialDataMgr : MonoBehaviour
    {
        [SerializeField] private OverallDataSO overallDataSO;
        [SerializeField] private GameObject UIGO;

        private async UniTaskVoid Awake()
        {
            await YooAssetResourceHub.Instance.LoadPackageAsync("StartPackage");
            var package = YooAssetResourceHub.Instance.GetPackage("StartPackage");
            if (package == null)
            {
                Debug.Log("StartPackage获取失败");
                return;
            }
            
            VisualElement BGIUI = UIGO.GetComponent<UIDocument>().rootVisualElement.
                    Q<VisualElement>("Background");

            var bgHandle = await YooAssetResourceHub.Instance.LoadAssetAsync<Texture2D>(package,
                "Assets/Resource/Start/StartBGI.png");
            
            if (bgHandle == null)
            {
                Debug.LogError($"背景图加载失败");
                return;
            }
            BGIUI.style.backgroundImage = bgHandle.GetAssetObject<Texture2D>();

            
            YooAssetResourceHub.Instance.Release(package);
            
            OverallData.CopyFieldsBySO(overallDataSO);
        }
    }
}
