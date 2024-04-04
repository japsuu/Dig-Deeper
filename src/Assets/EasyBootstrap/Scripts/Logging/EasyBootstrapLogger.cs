using UnityEngine;

namespace EasyBootstrap.Scripts.Logging
{
    public static class EasyBootstrapLogger
    {
#if UNITY_2022_2_OR_NEWER
        [HideInCallstack]
#endif
        public static void LogVerbose(string message, Object context = null)
        {
#if UNITY_EDITOR
            if(BootstrapSettings.Singleton.EnableVerboseLogging)
                Debug.Log($"<color=white>[EasyBootstrap]:</color> {message}", context);
#endif
        }
        
#if UNITY_2022_2_OR_NEWER
        [HideInCallstack]
#endif
        public static void Log(string message, Object context = null)
        {
            Debug.Log($"<color=black>[EasyBootstrap]:</color> {message}", context);
        }
        
        
#if UNITY_2022_2_OR_NEWER
        [HideInCallstack]
#endif
        public static void LogWarning(string message, Object context = null)
        {
            Debug.LogWarning($"<color=yellow>[EasyBootstrap]:</color> {message}", context);
        }


#if UNITY_2022_2_OR_NEWER
        [HideInCallstack]
#endif
        public static void LogError(string message, Object context = null)
        {
            Debug.LogError($"<color=red>[EasyBootstrap]:</color> {message}", context);
        }
    }
}