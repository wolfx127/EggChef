using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Overall
{
    public class OverallSceneManager : Singleton<OverallSceneManager>
    {
        public string startScene = "StartScene";
        public string constantScene = "ConstantScene";
        public string playScene = "PlayScene";
        public string settleScene = "SettleScene";
        public string failScene = "FailScene";

        public void FailToStart()
        {
            FailToStartTask().Forget();
        }
        private async UniTaskVoid FailToStartTask()
        {
            try
            {
                await LoadAsync(startScene).ToUniTask();
                await UnloadAsync(constantScene).ToUniTask();
                await UnloadAsync(failScene).ToUniTask();
            }
            catch (Exception e)
            {
                Debug.LogWarning("返回开始界面失败：" + '\n' + e.Message);
            }
        }

        public void StartToConstant()
        {
            StartToConstantTask().Forget();
        }
        private async UniTaskVoid StartToConstantTask()
        {
            try
            {
                await LoadAsync(constantScene).ToUniTask();
                await LoadAsync(playScene).ToUniTask();
                await UnloadAsync(startScene).ToUniTask();
            }
            catch (Exception e)
            {
                Debug.LogWarning("进入游戏失败：" + '\n' + e.Message);
            }
        }

        public void PlayToSettle()
        {
            PlayToSettleTask().Forget();
        }
        private async UniTaskVoid PlayToSettleTask()
        {
            try
            {
                await LoadAsync(settleScene).ToUniTask();
                await UnloadAsync(playScene).ToUniTask();
            }
            catch (Exception e)
            {
                Debug.LogWarning("进入结算失败：" + '\n' + e.Message);
            }
        }

        public void SettleToFail()
        {
            SettleToFailTask().Forget();
        }
        private async UniTaskVoid SettleToFailTask()
        {
            try
            {
                await LoadAsync(failScene).ToUniTask();
                await UnloadAsync(settleScene).ToUniTask();
            }
            catch (Exception e)
            {
                Debug.LogWarning("进入失败界面失败：" + '\n' + e.Message);
            }
        }

        public void SettleToPlay()
        {
            SettleToPlayTask().Forget();
        }
        private async UniTaskVoid SettleToPlayTask()
        {
            try
            {
                await LoadAsync(playScene).ToUniTask();
                await UnloadAsync(settleScene).ToUniTask();
            }
            catch (Exception e)
            {
                Debug.LogWarning("再来一局失败：" + '\n' + e.Message);
            }
        }

        private static AsyncOperation LoadAsync(string sceneName)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (op == null)
            {
                throw new Exception($"场景 {sceneName} 加载失败");
            }
            return op;
        }

        private static AsyncOperation UnloadAsync(string sceneName)
        {
            AsyncOperation op = SceneManager.UnloadSceneAsync(sceneName);
            if (op == null)
            {
                throw new Exception($"场景 {sceneName} 卸载失败");
            }
            return op;
        }
    }
}
