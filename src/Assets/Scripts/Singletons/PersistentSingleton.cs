using UnityEngine;

namespace Singletons
{
    /// <summary>
    /// Singleton object that will add itself as DontDestroyOnLoad.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PersistentSingleton<T> : MonoBehaviour where T : Component
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance != null) return instance;
            
                instance = FindObjectOfType<T>();
            
                if (instance != null) return instance;
            
                GameObject obj = new()
                {
                    name = typeof(T).Name
                };
                instance = obj.AddComponent<T>();
            
                Debug.LogWarning("Could not find the requested " + instance.GetType() + " in the scene. Creating...");
            
                return instance;
            }
        }
 
        protected virtual void Awake ()
        {
            if (instance == null)
            {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
