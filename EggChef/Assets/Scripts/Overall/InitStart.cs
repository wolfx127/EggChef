using System.Collections;
using Cysharp.Threading.Tasks;
using Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Overall
{
    public class InitStart : MonoBehaviour
    {
        //所以意味着constant场景必须永久存在，不能销毁再加载
        private async UniTaskVoid Start()
        {
            var op = SceneManager.LoadSceneAsync("StartScene", LoadSceneMode.Additive);

            if (op == null)
            {
                Debug.LogError("start场景加载失败");
                return;
            }

            await op.ToUniTask();
        }
    }
}
