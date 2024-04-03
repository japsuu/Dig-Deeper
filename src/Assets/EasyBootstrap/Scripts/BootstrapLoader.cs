using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EasyBootstrap.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace EasyBootstrap
{
    /// <summary>
    /// Makes sure bootstrap scene(s) get loaded first, no matter what scene the game is started in.
    /// Loads all objects from the Bootstrap scene(s), and calls Initialize() on objects inheriting <see cref="IBootstrappable"/>
    /// </summary>
    public static class BootstrapLoader
    {
        /// <summary>
        /// Called when bootstrapping is completed and post bootstrap scene is loaded.
        /// </summary>
        public static event Action BootstrappingCompleted;

        /// <summary>
        /// True if the first bootstrap scene has been loaded.
        /// </summary>
        public static bool IsFirstBootstrapSceneLoaded;

        private static string postBootstrapTargetScenePath = "";

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void BootstrapSceneCheck()
        {
            IsFirstBootstrapSceneLoaded = false;

            // Do not bootstrap if disabled.
            if (!BootstrapSettings.Singleton.EnableEditorBootstrapping)
            {
                EasyBootstrapLogger.LogWarning("Bootstrapping is disabled.\nRe-enable from toolbar -> 'Tools/EasyBootstrap/Settings'.");
                return;
            }

            // Do not bootstrap if no scenes to bootstrap.
            if (BootstrapSettings.Singleton.BootstrapScenePaths == null || BootstrapSettings.Singleton.BootstrapScenePaths.Count == 0)
            {
                EasyBootstrapLogger.LogWarning("No bootstrap scenes set.\nSet from toolbar -> 'Tools/EasyBootstrap/Settings'.");
                return;
            }

            // Cache reference to current scene if required.
            string activeScenePath = SceneManager.GetActiveScene().path;
            switch (BootstrapSettings.Singleton.EditorPostBootstrapHandlingType)
            {
                // If we are not loading any scene after bootstrapping, just execute all bootstraps.
                case EditorPostBootstrapHandlingType.StayInBootstrapScene:
                    postBootstrapTargetScenePath = "";
                    break;
                // If we are loading the current scene after bootstrapping, cache the current scene path.
                case EditorPostBootstrapHandlingType.LoadStartScene:
                    if(!BootstrapSettings.Singleton.BootstrapScenePaths.Contains(activeScenePath))
                        postBootstrapTargetScenePath = activeScenePath;
                    break;
                // If we are loading the post bootstrap scene after bootstrapping, cache the post bootstrap scene path.
                case EditorPostBootstrapHandlingType.LoadPostBootstrapScene:
                    if(!BootstrapSettings.Singleton.BootstrapScenePaths.Contains(BootstrapSettings.Singleton.EditorPostBootstrapScenePath))
                        postBootstrapTargetScenePath = BootstrapSettings.Singleton.EditorPostBootstrapScenePath;
                    else
                        EasyBootstrapLogger.LogWarning($"Editor post bootstrap scene {BootstrapSettings.Singleton.EditorPostBootstrapScenePath} is contained in bootstrap scenes list! This would cause an infinite loop.");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // If the bootstrap scene is already loaded, execute bootstrapping.
            if (IsBootstrapSceneLoaded())
            {
                if (SceneManager.GetActiveScene().isLoaded)
                    ExecuteBootstrapping();
                else
                    SceneManager.sceneLoaded += SceneManagerOnSceneLoaded;
                return;
            }

            SceneManager.sceneLoaded += SceneManagerOnSceneLoaded;

            EasyBootstrapLogger.LogVerbose("Disabling all loaded GameObjects to prevent Awake from being called on them.");

            // Disable loaded GOs to prevent Awake from being called on them.
            foreach (GameObject go in Object.FindObjectsOfType<GameObject>())
                go.SetActive(false);

            // Load the bootstrap scene.
            SceneManager.LoadScene(BootstrapSettings.Singleton.BootstrapScenePaths[0], LoadSceneMode.Single);
        }


        private static void SceneManagerOnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.path == BootstrapSettings.Singleton.BootstrapScenePaths[0])
                ExecuteBootstrapping();
        }
#endif


        private static bool IsBootstrapSceneLoaded()
        {
            // Go through currently loaded scenes.
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                // Initial bootstrap scene is at build index 0.
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.path == BootstrapSettings.Singleton.BootstrapScenePaths[0])
                    return true;
            }

            return false;
        }


#if !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
#endif
        private static async void ExecuteBootstrapping()
        {
            // Do not bootstrap if no scenes to bootstrap.
            if (BootstrapSettings.Singleton.BootstrapScenePaths == null || BootstrapSettings.Singleton.BootstrapScenePaths.Count == 0)
            {
                EasyBootstrapLogger.LogWarning("No bootstrap scenes set.\nCannot bootstrap.");
                return;
            }
            
            EasyBootstrapLogger.LogVerbose("Executing bootstraps.");

            // At this point in time, the current loaded scene path is BootstrapSettings.Singleton.BootstrapScenes[0] (the initial bootstrap scene).
            try
            {
                // Fetch the settings object.
                BootstrapSettings settings = BootstrapSettings.Singleton;

                // Initialize the main bootstrap scene.
                Scene initialBootstrapScene = SceneManager.GetSceneByPath(BootstrapSettings.Singleton.BootstrapScenePaths[0]);

                // Initialize all bootstrap components one by one.
                await InitializeBootstrapComponents(initialBootstrapScene);

                IsFirstBootstrapSceneLoaded = true;

                if (settings == null)
                {
                    EasyBootstrapLogger.LogWarning($"Could not fetch {typeof(BootstrapSettings)}!");
                }
                else
                {
                    // Load & Bootstrap all additional scenes too.
                    if (settings.BootstrapScenePaths != null)
                    {
                        foreach (string scenePath in settings.BootstrapScenePaths)
                        {
                            // Skip the first bootstrap scene, as it's already loaded.
                            if(scenePath == BootstrapSettings.Singleton.BootstrapScenePaths[0])
                                continue;
                            
                            await SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);

                            // Initialize all bootstrap components one by one.
                            await InitializeBootstrapComponents(SceneManager.GetSceneByPath(scenePath));
                        }
                    }
                }
                
                #if UNITY_EDITOR
                
                // ALL Bootstraps loaded, now load the wanted scene.
                if (string.IsNullOrEmpty(postBootstrapTargetScenePath))
                {
                    if (settings.BootstrapScenePaths != null)
                        SceneManager.SetActiveScene(SceneManager.GetSceneByPath(settings.BootstrapScenePaths.Last()));
                }
                else
                {
                    await SceneManager.LoadSceneAsync(postBootstrapTargetScenePath, LoadSceneMode.Additive);
                    SceneManager.SetActiveScene(SceneManager.GetSceneByPath(postBootstrapTargetScenePath));
                }
                
                #else
                
                if(settings.BuildPostBootstrapHandlingType == BuildPostBootstrapHandlingType.LoadPostBootstrapScene)
                {
                    postBootstrapTargetScenePath = BootstrapSettings.Singleton.PostBootstrapScenePath;
                    
                    await SceneManager.LoadSceneAsync(postBootstrapTargetScenePath, LoadSceneMode.Additive);
                    SceneManager.SetActiveScene(SceneManager.GetSceneByPath(postBootstrapTargetScenePath));
                }
                
                #endif

                BootstrappingCompleted?.Invoke();
            }
            catch (Exception e)
            {
                EasyBootstrapLogger.LogError($"Could not bootstrap :(\n{e.Message}");
            }
        }


        private static async Task InitializeBootstrapComponents(Scene scene)
        {
            // Get the bootstraps from the bootstrap scene.
            List<IBootstrappable> bootstraps = GetBootstraps(scene);

            Stopwatch stopwatch = new();

            // Go through the bootstraps.
            foreach (IBootstrappable bootstrap in bootstraps)

                // Initialize bootstrap.
                try
                {
                    stopwatch.Start();
                    await bootstrap.Initialize();
                    stopwatch.Stop();

                    EasyBootstrapLogger.LogVerbose($"[{bootstrap.GetType().Name}] initialized in {stopwatch.ElapsedMilliseconds}ms");
                }
                catch (Exception e)
                {
                    EasyBootstrapLogger.LogError($"Could not initialize {bootstrap.GetType().Name}: {e}");
                    throw;
                }
        }


        /// <returns>All objects inheriting <see cref="IBootstrappable"/> in the provided scene.</returns>
        private static List<IBootstrappable> GetBootstraps(Scene scene)
        {
            GameObject[] rootGameObjects = scene.GetRootGameObjects();
            return rootGameObjects
                .Where(gameObject => gameObject != null)
                .SelectMany(gameObject => gameObject.GetComponentsInChildren<IBootstrappable>())
                .OrderBy(x => x.BootstrapCallOrder).ToList();
        }


        /// <summary>
        /// Allows awaiting <see cref="AsyncOperation"/>s.
        /// </summary>
        private static TaskAwaiter GetAwaiter(this AsyncOperation asyncOp)
        {
            // There is a GC hit here, but it's not too bad. You could also use CySharp's AwaiterExtensions for no GC hit.
            TaskCompletionSource<AsyncOperation> tcs = new();
            asyncOp.completed += operation => { tcs.SetResult(operation); };
            return ((Task)tcs.Task).GetAwaiter();
        }
    }
}