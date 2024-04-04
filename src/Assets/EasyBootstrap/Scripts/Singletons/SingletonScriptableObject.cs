using EasyBootstrap.Scripts.Logging;
using UnityEngine;

namespace EasyBootstrap.Scripts.Singletons
{
    public abstract class SingletonScriptableObject<T> : ScriptableObject where T : ScriptableObject
    {
        private const string SINGLETON_DEFAULT_PATH = "Assets/EasyBootstrap/Resources";
        
        private static T singleton;

        public static T Singleton
        {
            get
            {
                if (singleton != null)
                    return singleton;
                
                T[] results = Resources.LoadAll<T>("EasyBootstrap.BootstrapSettings");
                
                switch (results.Length)
                {
                    case 0:
                    {
                        EasyBootstrapLogger.LogWarning($"Could not find the ScriptableObject of type {typeof(T)}!");
                        // Create a new instance.  
                        // This is not ideal, but it's better than crashing the game.
                        singleton = CreateInstance<T>();
                        singleton.hideFlags = HideFlags.DontUnloadUnusedAsset;
                        
                        // If we are in editor, save the instance to resources folder.
                        #if UNITY_EDITOR
                        UnityEditor.AssetDatabase.CreateAsset(singleton, $"{SINGLETON_DEFAULT_PATH}/{typeof(T)}.asset");
                        
                        EasyBootstrapLogger.LogWarning($"Created a new ScriptableObject of type {typeof(T)} to {SINGLETON_DEFAULT_PATH}.");
                        #endif
                        
                        return null;
                    }
                    case > 1:
                        EasyBootstrapLogger.LogWarning($"Found multiple ScriptableObjects of type {typeof(T)}!");
                        break;
                }

                singleton = results[0];
                singleton.hideFlags = HideFlags.DontUnloadUnusedAsset;
                
                return singleton;
            }
        }
    }
}