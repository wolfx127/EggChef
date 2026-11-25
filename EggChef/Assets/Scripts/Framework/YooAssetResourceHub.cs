using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using YooAsset;

namespace Framework
{
    //资源包的引用计数和加载界面
    public class YooAssetResourceHub : MonoSingleton<YooAssetResourceHub>
    {
        [SerializeField] private PanelSettings panelSettings;

        private class Entry
        {
            public ResourcePackage Package;
            public int RefCount;
        }

        private readonly Dictionary<string, Entry> entries = new();
        private readonly Dictionary<string, UniTask<ResourcePackage>> loading = new();

        private UIDocument document;
        private VisualElement progressRoot;
        private ProgressBar progressBar;
        private Label progressLabel;

        protected override void Awake()
        {
            base.Awake();

            if (Instance == this)
                BuildUI();
        }

        public async UniTask<ResourcePackage> AcquireAsync(string key)
        {
            if (entries.TryGetValue(key, out var entry))
            {
                entry.RefCount++;
                return entry.Package;
            }

            if (loading.TryGetValue(key, out var task))
            {
                var loadingPackage = await task;
                if (loadingPackage != null)
                    entries[key].RefCount++;

                return loadingPackage;
            }

            task = LoadAndRegisterAsync(key);
            loading[key] = task;

            try
            {
                return await task;
            }
            finally
            {
                loading.Remove(key);
            }
        }

        public bool TryGet(string key, out ResourcePackage package)
        {
            if (entries.TryGetValue(key, out var entry))
            {
                package = entry.Package;
                return true;
            }

            package = null;
            return false;
        }

        public void Release(string key)
        {
            ReleaseAsync(key).Forget();
        }

        public async UniTask ReleaseAsync(string key)
        {
            if (!entries.TryGetValue(key, out var entry))
                return;

            entry.RefCount--;

            if (entry.RefCount > 0)
                return;

            entries.Remove(key);

            var op = entry.Package.DestroyPackageAsync();
            await op;

            if (op.Status != EOperationStatus.Succeeded)
            {
                Debug.LogError($"[ResourceHub] 销毁失败: {op.Error}");
                return;
            }

            YooAssets.RemovePackage(entry.Package.PackageName);
        }

        private async UniTask<ResourcePackage> LoadAndRegisterAsync(string key)
        {
            var tcs = new UniTaskCompletionSource<ResourcePackage>();

            StartCoroutine(YooAssetMgr.LoadPackageAsync(
                key,
                package => tcs.TrySetResult(package),
                ReportProgress));

            var package = await tcs.Task;
            if (progressRoot != null)
                progressRoot.style.display = DisplayStyle.None;

            if (package == null)
                return null;

            entries[key] = new Entry
            {
                Package = package,
                RefCount = 1
            };

            return package;
        }

        private void BuildUI()
        {
            if (panelSettings == null)
                panelSettings = Resources.Load<PanelSettings>("YooAssetLoadingPanelSettings");

            if (panelSettings == null)
            {
                Debug.LogError("[ResourceHub] 缺少 PanelSettings");
                return;
            }

            var go = new GameObject("LoadingUI");
            go.transform.SetParent(transform, false);

            document = go.AddComponent<UIDocument>();
            document.panelSettings = panelSettings;
            document.sortingOrder = 30000;

            progressRoot = new VisualElement();
            progressRoot.style.position = Position.Absolute;
            progressRoot.style.left = 0;
            progressRoot.style.right = 0;
            progressRoot.style.top = 0;
            progressRoot.style.bottom = 0;
            progressRoot.style.backgroundColor = new Color(0f, 0f, 0f, 0.65f);

            var box = new VisualElement();
            box.style.width = 600;
            box.style.alignSelf = Align.Center;
            box.style.marginTop = Length.Percent(40);

            progressLabel = new Label("加载中...");
            progressLabel.style.color = Color.white;
            progressLabel.style.unityTextAlign = TextAnchor.MiddleCenter;

            progressBar = new ProgressBar();
            progressBar.lowValue = 0;
            progressBar.highValue = 100;
            progressBar.value = 0;

            box.Add(progressLabel);
            box.Add(progressBar);
            progressRoot.Add(box);

            document.rootVisualElement.Add(progressRoot);

            if (progressRoot != null)
                progressRoot.style.display = DisplayStyle.None;
        }

        private void ReportProgress(float value, string text)
        {
            if (progressRoot != null)
                progressRoot.style.display = DisplayStyle.Flex;

            if (progressBar != null)
                progressBar.value = value;

            if (progressLabel != null)
                progressLabel.text = text;
        }
    }
}
