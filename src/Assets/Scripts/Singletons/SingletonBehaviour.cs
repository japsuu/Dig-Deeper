using UnityEngine;

// ReSharper disable StaticMemberInGenericType
// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace Singletons
{
    /// <summary>
    /// Instance object, that can be used from anywhere by calling ClassNameHere.Instance.
    /// Only one singleton of some type can exist at once.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        // Check to see if we're about to be destroyed.
        private static bool shuttingDown;
        private static object lockObj = new();
        private static T instance;

        public static T Instance
        {
            get
            {
                if (!Application.isPlaying) return null;
            
                if (shuttingDown)
                {
                    //Debug.LogWarning("[Instance] Instance '" + typeof(T) +
                    //    "' already destroyed. Returning null.");
                    return null;
                }

                lock (lockObj)
                {
                    if (instance == null)
                    {
                        instance = (T)FindObjectOfType(typeof(T));
                    }

                    return instance;
                }
            }
        }
        
        private void OnEnable()
        {
            shuttingDown = false;
        }


        private void OnApplicationQuit()
        {
            shuttingDown = true;
        }


        private void OnDestroy()
        {
            shuttingDown = true;
        }
    }
}