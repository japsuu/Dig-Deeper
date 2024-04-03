using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace EasyBootstrap.Editor
{
    public class EasyBootstrapBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;


        public void OnPreprocessBuild(BuildReport report)
        {
            if (BootstrapSettings.Singleton.BuildPostBootstrapHandlingType == BuildPostBootstrapHandlingType.LoadPostBootstrapScene && string.IsNullOrEmpty(BootstrapSettings.Singleton.PostBootstrapScenePath))
            {
                BootstrapSettings.Singleton.EditorPostBootstrapHandlingType = EditorPostBootstrapHandlingType.LoadPostBootstrapScene;
                throw new BuildFailedException("Post-bootstrap scene is not set. Please either assign a scene to load from toolbar -> 'Tools/EasyBootstrap/Settings' -> 'build post-bootstrap scene', or change the after bootstrapping handling type.");
            }
        }
    }
}