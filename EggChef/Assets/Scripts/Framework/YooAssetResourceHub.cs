using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using YooAsset;

namespace Framework
{
    // 资源包的唯一入口（调用者不要直接碰 YooAssetMgr）：
    //   - 引用计数，把包交给调用者
    //   - 加载界面：加载中一律显示"资源加载中..."，只有失败才显示"资源加载失败"
    //   - 空闲包自动清理
    public class YooAssetResourceHub : MonoSingleton<YooAssetResourceHub>
    {
        private const float CleanupInterval = 30f;
        private const float IdleSeconds = 180f;

        private const string LoadingText = "资源加载中...";
        private const string FailedText = "资源加载失败";

        [SerializeField] private PanelSettings panelSettings;

        private class Entry
        {
            public ResourcePackage Package;
            public int RefCount;
            public float LastAccessTime;
            public bool IsUnloading;
        }

        private readonly Dictionary<string, Entry> entries = new();
        private readonly Dictionary<string, UniTask<ResourcePackage>> loading = new();

        // 正在加载的各个包的进度（0~1），加载条显示它们的平均值
        private readonly Dictionary<string, float> progress = new();

        private UIDocument document;
        private VisualElement progressRoot;
        private ProgressBar progressBar;
        private Label progressLabel;

        private bool overlayVisible;
        private bool errorShown;

        protected override void Awake()
        {
            base.Awake();

            if (Instance == this)
                BuildUI();
        }

        // 界面的一切（显隐、文案、进度）都只由状态推导，
        // 没有任何地方手动开关，所以不会漏关。
        private void LateUpdate()
        {
            if (progressRoot == null)
                return;

            bool shouldShow = loading.Count > 0 || errorShown;

            if (shouldShow != overlayVisible)
            {
                overlayVisible = shouldShow;
                progressRoot.style.display =
                    shouldShow ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (!shouldShow)
                return;

            progressLabel.text = errorShown ? FailedText : LoadingText;
            progressBar.value = errorShown ? 0f : AverageProgress() * 100f;
        }

        private async UniTaskVoid Start()
        {
            var token = this.GetCancellationTokenOnDestroy();

            try
            {
                while (true)
                {
                    await UniTask.Delay(
                        TimeSpan.FromSeconds(CleanupInterval),
                        DelayType.UnscaledDeltaTime,
                        cancellationToken: token);

                    try
                    {
                        await CleanupUnusedAsync();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[ResourceHub] 清理出错: {e}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        // ==================== 对外 API ====================

        // 只加载，不增加引用计数。成功返回 true。
        public async UniTask<bool> LoadPackageAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("[ResourceHub] 包名为空");
                return false;
            }

            // 正在销毁的包：等它销毁完，再重新加载
            while (entries.TryGetValue(key, out var unloading) && unloading.IsUnloading)
                await UniTask.Yield();

            // 已经加载好
            if (entries.TryGetValue(key, out var entry))
            {
                entry.LastAccessTime = Time.realtimeSinceStartup;
                return true;
            }

            // 别人正在加载同一个包：一起等
            if (loading.TryGetValue(key, out var running))
                return (await running) != null;

            // 同一个任务会被多个调用者 await，必须 Preserve
            var task = LoadAndRegisterAsync(key).Preserve();
            loading[key] = task;

            try
            {
                return (await task) != null;
            }
            finally
            {
                loading.Remove(key);
            }
        }

        // 只拿已经加载好的包，引用计数 +1。用完必须 Release。
        public ResourcePackage GetPackage(string key)
        {
            if (!entries.TryGetValue(key, out var entry))
                return null;

            if (entry.IsUnloading)
                return null;

            entry.RefCount++;
            entry.LastAccessTime = Time.realtimeSinceStartup;
            return entry.Package;
        }

        // 只释放引用，不销毁包；0 引用后由计时器清理
        public void Release(string key)
        {
            if (!entries.TryGetValue(key, out var entry))
                return;

            if (entry.RefCount <= 0)
                return;

            entry.RefCount--;
            entry.LastAccessTime = Time.realtimeSinceStartup;
        }

        public void Release(ResourcePackage package)
        {
            if (package == null)
                return;

            Release(package.PackageName);
        }

        // 从包里加载资源。成功返回 handle（用完 handle.Release()），失败返回 null。
        public async UniTask<AssetHandle> LoadAssetAsync<T>(
            ResourcePackage package, string location)
            where T : UnityEngine.Object
        {
            if (package != null && entries.TryGetValue(package.PackageName, out var entry))
                entry.LastAccessTime = Time.realtimeSinceStartup;

            return await YooAssetMgr.LoadAssetAsync<T>(package, location);
        }

        // 强制释放所有包（不管引用计数），用于退出游戏/彻底清场
        public async UniTask ReleaseAllAsync()
        {
            while (loading.Count > 0)
                await UniTask.Yield();

            foreach (var key in new List<string>(entries.Keys))
            {
                while (entries.TryGetValue(key, out var unloading) && unloading.IsUnloading)
                    await UniTask.Yield();

                await UnloadAsync(key, true);
            }
        }

        // ==================== 内部 ====================

        private async UniTask<ResourcePackage> LoadAndRegisterAsync(string key)
        {
            errorShown = false;
            progress[key] = 0f;

            bool finished = false;
            ResourcePackage package = null;

            try
            {
                package = await YooAssetMgr.LoadPackageAsync(key, value =>
                {
                    if (!finished)
                        progress[key] = value;
                });
            }
            catch (Exception e)
            {
                Debug.LogError($"[ResourceHub] {key} 加载异常: {e}");
            }
            finally
            {
                finished = true;
                progress.Remove(key);
            }

            if (package == null)
            {
                // 失败停在错误提示上，直到下一次加载开始
                errorShown = true;
                return null;
            }

            entries[key] = new Entry
            {
                Package = package,
                RefCount = 0,
                LastAccessTime = Time.realtimeSinceStartup
            };

            return package;
        }

        private async UniTask CleanupUnusedAsync()
        {
            float now = Time.realtimeSinceStartup;
            var keys = new List<string>();

            foreach (var pair in entries)
            {
                var e = pair.Value;

                if (e.IsUnloading || e.RefCount > 0)
                    continue;

                if (now - e.LastAccessTime < IdleSeconds)
                    continue;

                keys.Add(pair.Key);
            }

            foreach (var key in keys)
                await UnloadAsync(key, false);
        }

        private async UniTask UnloadAsync(string key, bool force)
        {
            if (!entries.TryGetValue(key, out var entry))
                return;

            if (entry.IsUnloading)
                return;

            if (!force && entry.RefCount > 0)
                return;

            entry.IsUnloading = true;

            bool ok = false;
            try
            {
                ok = await YooAssetMgr.ReleasePackageAsync(entry.Package);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ResourceHub] 销毁异常 {key}: {e}");
            }

            if (!ok)
            {
                entry.IsUnloading = false;
                return;
            }

            entries.Remove(key);
        }

        private float AverageProgress()
        {
            if (progress.Count == 0)
                return 0f;

            float sum = 0f;
            foreach (var value in progress.Values)
                sum += value;

            return sum / progress.Count;
        }

        private void BuildUI()
        {
            if (document != null)
                return;

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

            progressLabel = new Label(LoadingText);
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

            overlayVisible = false;
            progressRoot.style.display = DisplayStyle.None;
        }
    }
}
