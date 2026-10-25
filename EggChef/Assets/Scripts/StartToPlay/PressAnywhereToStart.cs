using Overall;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StartToPlay
{
    public class PressAnywhereToStart : MonoBehaviour
    {
        private Mouse mouse;
        private Keyboard keyboard;
        private bool isLoading;

        private void Awake()
        {
            mouse = Mouse.current;
            keyboard = Keyboard.current;
        }

        private void Update()
        {
            if (isLoading) return;

            bool pressed = (mouse != null && (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame))
                        || (keyboard != null && keyboard.anyKey.wasPressedThisFrame);
            if (!pressed) return;

            isLoading = true;
            Debug.Log("点击进入游戏");

            // 场景切换全部交给 OverallSceneManager，这里只负责"按了什么"
            OverallSceneManager.Instance.StartToConstant();
        }
    }
}
