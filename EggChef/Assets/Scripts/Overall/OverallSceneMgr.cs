using System;
using Cysharp.Threading.Tasks;
using Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Overall
{
    // 场景加载/卸载 + 场景加载条。不负责资源包。
    // 两个 API：
    //   ChangeScene  加载新场景（Additive）、设为活动场景、卸载旧场景
    //   DeleteScene  卸载单个场景
    // 加载条的文案一律是"加载中..."
    public class OverallSceneMgr : MonoSingleton<OverallSceneMgr>
    {
        private const string LoadingText = "加载中...";

        [SerializeField] private PanelSettings panelSettings;

        private UIDocument document;
        private VisualElement progressRoot;
        private ProgressBar progressBar;

        private bool changing;

        protected override void Awake()
        {
            base.Awake();

            if (Instance == this)
                BuildUI();
        }

        // ==================== API 1：切换场景 ====================
        // loadName 加载并设为活动场景；deleteName 不为空则在加载完成后卸载
        public async UniTask ChangeScene(string loadName, string deleteName)
        {
            if (changing)
            {
                Debug.LogWarning("[SceneMgr] 场景正在切换，忽略本次请求");
                return;
            }

            if (string.IsNullOrEmpty(loadName))
            {
                Debug.LogError("[SceneMgr] 场景名为空");
                return;
            }

            changing = true;
            SetOverlay(true);

            try
            {
                if (!SceneManager.GetSceneByName(loadName).isLoaded)
                {
                    var loadOp = SceneManager.LoadSceneAsync(
                        loadName, LoadSceneMode.Additive);

                    if (loadOp == null)
                    {
                        Debug.LogError($"[SceneMgr] 场景加载失败: {loadName}");
                        return;
                    }

                    // Unity 的 progress 到 0.9 就算加载完成
                    while (!loadOp.isDone)
                    {
                        Report(Mathf.Clamp01(loadOp.progress / 0.9f));
                        await UniTask.Yield();
                    }
                }

                Report(1f);

                var loadedScene = SceneManager.GetSceneByName(loadName);
                if (loadedScene.IsValid())
                    SceneManager.SetActiveScene(loadedScene);

                if (!string.IsNullOrEmpty(deleteName) && deleteName != loadName)
                {
                    if (SceneManager.GetSceneByName(deleteName).isLoaded)
                    {
                        var unloadOp = SceneManager.UnloadSceneAsync(deleteName);
                        if (unloadOp != null)
                            await unloadOp.ToUniTask();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SceneMgr] 场景切换失败: {e}");
            }
            finally
            {
                SetOverlay(false);
                changing = false;
            }
        }

        // ==================== API 2：删除单个场景 ====================
        public async UniTask DeleteScene(string sceneName)
        {
            if (changing)
            {
                Debug.LogWarning("[SceneMgr] 场景正在切换，忽略本次请求");
                return;
            }

            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[SceneMgr] 场景名为空");
                return;
            }

            if (!SceneManager.GetSceneByName(sceneName).isLoaded)
            {
                Debug.LogWarning($"[SceneMgr] 场景没有加载，无需删除: {sceneName}");
                return;
            }

            changing = true;
            SetOverlay(true);

            try
            {
                var unloadOp = SceneManager.UnloadSceneAsync(sceneName);

                if (unloadOp == null)
                {
                    Debug.LogError($"[SceneMgr] 场景卸载失败: {sceneName}");
                    return;
                }

                while (!unloadOp.isDone)
                {
                    Report(Mathf.Clamp01(unloadOp.progress));
                    await UniTask.Yield();
                }

                Report(1f);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SceneMgr] 场景删除失败: {e}");
            }
            finally
            {
                SetOverlay(false);
                changing = false;
            }
        }

        // ==================== 加载条 ====================
        private void SetOverlay(bool visible)
        {
            if (progressRoot == null)
                return;

            if (visible && progressBar != null)
                progressBar.value = 0f;

            progressRoot.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void Report(float value)
        {
            if (progressBar != null)
                progressBar.value = value * 100f;
        }

        private void BuildUI()
        {
            if (panelSettings == null)
                panelSettings = Resources.Load<PanelSettings>("YooAssetLoadingPanelSettings");

            if (panelSettings == null)
            {
                Debug.LogError("[SceneMgr] 缺少 PanelSettings");
                return;
            }

            var go = new GameObject("SceneLoadingUI");
            go.transform.SetParent(transform, false);

            document = go.AddComponent<UIDocument>();
            document.panelSettings = panelSettings;
            document.sortingOrder = 29999;

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

            var label = new Label(LoadingText);
            label.style.color = Color.white;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;

            progressBar = new ProgressBar();
            progressBar.lowValue = 0;
            progressBar.highValue = 100;
            progressBar.value = 0;

            box.Add(label);
            box.Add(progressBar);
            progressRoot.Add(box);

            document.rootVisualElement.Add(progressRoot);
            progressRoot.style.display = DisplayStyle.None;
        }
    }
}
