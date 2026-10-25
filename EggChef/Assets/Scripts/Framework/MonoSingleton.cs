using UnityEngine;

namespace Overall
{
    [DisallowMultipleComponent]
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (!instance)
                    instance = FindFirstObjectByType<T>(FindObjectsInactive.Include);
                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (instance && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = (T)this;

            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }

            OnInstanceReady();
        }

        protected virtual void OnInstanceReady()
        {
        }

        protected virtual void OnDestroy()
        {
            if (instance == this) instance = null;
        }
    }
}
