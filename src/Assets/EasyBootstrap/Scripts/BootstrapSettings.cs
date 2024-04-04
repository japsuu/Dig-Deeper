using System.Collections.Generic;
using EasyBootstrap.Scripts.Singletons;
using UnityEngine;

namespace EasyBootstrap.Scripts
{
    /// <summary>
    /// Placed in the first loaded scene (main bootstrap scene),
    /// where <see cref="BootstrapLoader"/> will load this object to access it's settings.
    /// </summary>
    public class BootstrapSettings : SingletonScriptableObject<BootstrapSettings>
    {
#if UNITY_EDITOR
        [HideInInspector]
        public bool EnableEditorBootstrapping = true;
        
        public bool EnableVerboseLogging;
        [HideInInspector]
        public EditorPostBootstrapHandlingType EditorPostBootstrapHandlingType = EditorPostBootstrapHandlingType.LoadStartScene;
        [HideInInspector]
        public string EditorPostBootstrapScenePath;
#endif
        [HideInInspector]
        [Tooltip("The type of handling to be done after all bootstrapping is done when in a built project.")]
        public BuildPostBootstrapHandlingType BuildPostBootstrapHandlingType = BuildPostBootstrapHandlingType.LoadPostBootstrapScene;
        
        [HideInInspector]
        [Tooltip("The scene to be loaded after all bootstrapping is done. This should be your gameplay scene.")]
        public string PostBootstrapScenePath;
        
        [HideInInspector]
        [Tooltip("All scenes that should be loaded before the gameplay scene. Loaded in the order they appear in this list.")]
        public List<string> BootstrapScenePaths = new();


/*#if UNITY_EDITOR
        private void OnValidate()
        {
            if(string.IsNullOrEmpty(PostBootstrapScenePath))
                return;
            
            // Ensure that the post-bootstrap scene is not in the bootstrap scenes list.
            if (!BootstrapScenePaths.Contains(PostBootstrapScenePath))
                return;
            
            Logging.EasyBootstrapLogger.LogError("Post-bootstrap scene was contained in bootstrap scenes. This is not allowed, and it was removed from the bootstrap scenes list.");
            BootstrapScenePaths.Remove(PostBootstrapScenePath);
        }
#endif*/
    }
}