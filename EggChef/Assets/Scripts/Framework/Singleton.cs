using System;

namespace Framework
{
    public abstract class Singleton<T> where T : Singleton<T>, new()
    {
        private static readonly Lazy<T> lazy = new Lazy<T>(CreateInstance);

        public static T Instance => lazy.Value;

        private static T CreateInstance()
        {
            var instance = new T();
            instance.OnInit();
            return instance;
        }
        
        protected virtual void OnInit()
        {
        }
    }
}
