using System;
using System.Linq;
using EasyBootstrap.Logging;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace EasyBootstrap.Editor
{
    public class BootstrapEditorWindow : EditorWindow
    {
        private GUIStyle _richLabelStyle;
        private GUIStyle _titleLabelStyle;
        private Vector2 _scrollPosition = Vector2.zero;
        private SerializedObject _objectSo;
        private ReorderableList _scenesRl;


        [MenuItem("Tools/EasyBootstrap/Settings", priority = -99)]
        public static void ShowWindow()
        {
            Type desiredDockWindow = Type.GetType("UnityEditor.InspectorWindow,UnityEditor.dll");
            BootstrapEditorWindow window = GetWindow<BootstrapEditorWindow>("EasyBootstrap Settings", true, desiredDockWindow);
            window.Initialize();
        }


        private void OnGUI()
        {
            EditorGUI.indentLevel++;
            DrawBootstrappingSettings();
            EditorGUI.indentLevel--;
        }


        private void Update()
        {
            EditorUtility.SetDirty(BootstrapSettings.Singleton);
        }


        private void OnInspectorUpdate()
        {
            Repaint();
        }


        private void InitializeStyles()
        {
            _titleLabelStyle ??= new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal =
                {
                    textColor = Color.white
                }
            };
        }


        private void Initialize()
        {
            InitializeStyles();

            _objectSo = new SerializedObject(BootstrapSettings.Singleton);
            
            _scenesRl = new ReorderableList(_objectSo, _objectSo.FindProperty("BootstrapScenePaths"), true, true, true, true)
            {
                drawHeaderCallback = DrawHeaderCallback,
                drawElementCallback = DrawElementCallback
            };
            
            EditorBuildSettings.sceneListChanged -= Initialize;
            EditorBuildSettings.sceneListChanged += Initialize;
        }


        private void DrawHeaderCallback(Rect rect)
        {
            GUIContent content = new("Bootstrap scenes ⓘ", "The scenes that are considered bootstrap scenes by EasyBootstrap, and loaded on entering play mode.")
            {
                image = EditorGUIUtility.IconContent("BuildSettings.Editor").image
            };
            EditorGUI.LabelField(rect, content);
        }


        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.BeginChangeCheck();

            DrawSceneAssetFieldForList(index, rect);

            EditorGUI.EndChangeCheck();
        }


        private void OnDestroy()
        {
            EditorBuildSettings.sceneListChanged -= Initialize;
        }


        private void DrawBootstrappingSettings()
        {
            if (_scenesRl == null)
                Initialize();

            // This should not happen, but just in case :D.
            if (BootstrapSettings.Singleton == null)
                return;

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, false, false);
            
            DrawSectionWarnings();
            
            EditorGUILayout.Space(20f);

            DrawSectionGeneral();

            DrawSpacerLine();

            DrawSectionBuild();

            DrawSpacerLine();

            DrawSectionEditor();

            DrawSpacerLine();

            DrawSectionDebug();

            DrawSpacerLine();

            // EditorGUILayout.LabelField("Additional info:", _titleLabelStyle, GUILayout.Height(30f));
            // EditorGUILayout.LabelField(
            //
            //     //"- Please ensure that your bootstrap scenes are assigned to the build settings in the same order as here.\n\n" +
            //     "- Make sure your possible gameplay scenes are assigned to the build settings AFTER the bootstrap scenes.",
            //     EditorStyles.wordWrappedLabel);
            //
            // DrawSpacerLine(Color.black);

            EditorGUILayout.EndScrollView();
        }


        private static void DrawSectionWarnings()
        {
            // If post-bootstrap scenes exist in the bootstrap scenes list, show a warning.
            bool isAnyPostBootstrapContainedInBootstraps = BootstrapSettings.Singleton.BootstrapScenePaths != null &&
                                                        ((!string.IsNullOrEmpty(BootstrapSettings.Singleton.PostBootstrapScenePath) &&
                                                          BootstrapSettings.Singleton.BootstrapScenePaths.Contains(BootstrapSettings.Singleton.PostBootstrapScenePath)) ||
                                                         (!string.IsNullOrEmpty(BootstrapSettings.Singleton.EditorPostBootstrapScenePath) &&
                                                          BootstrapSettings.Singleton.BootstrapScenePaths.Contains(BootstrapSettings.Singleton.EditorPostBootstrapScenePath)));
            
            bool areAllBootstrapScenesInBuildSettings = BootstrapSettings.Singleton.BootstrapScenePaths != null && BootstrapSettings.Singleton.BootstrapScenePaths.All(s => string.IsNullOrEmpty(s) || IsSceneInBuildSettings(s));

            bool hasAssignedBootstrapScenes = BootstrapSettings.Singleton.BootstrapScenePaths != null && BootstrapSettings.Singleton.BootstrapScenePaths.Count(s => !string.IsNullOrEmpty(s)) != 0;
            bool shouldLoadPostSceneBuild = BootstrapSettings.Singleton.BuildPostBootstrapHandlingType == BuildPostBootstrapHandlingType.LoadPostBootstrapScene;
            bool hasValuePostSceneBuild = !string.IsNullOrEmpty(BootstrapSettings.Singleton.PostBootstrapScenePath);
            bool existsPostSceneBuild = hasValuePostSceneBuild && DoesSceneExist(BootstrapSettings.Singleton.PostBootstrapScenePath);
            bool isBuildPostSceneInBuildSettings = IsSceneInBuildSettings(BootstrapSettings.Singleton.PostBootstrapScenePath);

            // Error if post-bootstrap scenes exist in the bootstrap scenes list.
            if (isAnyPostBootstrapContainedInBootstraps)
                EditorGUILayout.HelpBox("A post-bootstrap scene is contained in the bootstrap scenes list! This would cause an infinite loop!", MessageType.Error);
            
            // Error if no bootstrap scenes are assigned.
            if(!hasAssignedBootstrapScenes)
                EditorGUILayout.HelpBox("No bootstrap scenes assigned!", MessageType.Error);
            
            // Error if the build post-bootstrap scene is not assigned.
            if(shouldLoadPostSceneBuild && !hasValuePostSceneBuild)
                EditorGUILayout.HelpBox("BUILD Post-bootstrap scene is not assigned.", MessageType.Error);

            // Error if the assigned build post-bootstrap scene does not exist.
            if (shouldLoadPostSceneBuild && hasValuePostSceneBuild && !existsPostSceneBuild)
                EditorGUILayout.HelpBox("Assigned BUILD post-bootstrap scene does not exist.", MessageType.Error);

            // Warning if the first bootstrap scene is not at build index 0.
            if (areAllBootstrapScenesInBuildSettings && hasAssignedBootstrapScenes)
            {
                int firstBootstrapSceneIndex = GetSceneBuildSettingsIndex(BootstrapSettings.Singleton.BootstrapScenePaths[0]);
                if (firstBootstrapSceneIndex != 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.HelpBox("The first bootstrap scene should be at build index 0 in build settings!", MessageType.Warning);

                    if (GUILayout.Button("Fix", GUILayout.Width(50f), GUILayout.Height(EditorGUIUtility.singleLineHeight * 2f)))
                    {
                        ChangeSceneBuildSettingsIndex(firstBootstrapSceneIndex, 0);
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            // Warning if all bootstrap scenes are not in the build settings.
            if (hasAssignedBootstrapScenes && !areAllBootstrapScenesInBuildSettings)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox("Not all bootstrap scenes are added to the build settings!", MessageType.Error);

                if (GUILayout.Button("Fix", GUILayout.Width(50f), GUILayout.Height(EditorGUIUtility.singleLineHeight * 2f)))
                {
                    foreach (string scenePath in BootstrapSettings.Singleton.BootstrapScenePaths.Where(scenePath => !IsSceneInBuildSettings(scenePath)))
                    {
                        InsertSceneToBuildSettings(scenePath);
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            // Warning if the post-bootstrap scene is not in the build settings.
            if (existsPostSceneBuild && !isBuildPostSceneInBuildSettings)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox("Assigned BUILD post-bootstrap scene is not in build settings.", MessageType.Warning);
                        
                if (GUILayout.Button("Fix", GUILayout.Width(50f), GUILayout.Height(EditorGUIUtility.singleLineHeight * 2f)))
                {
                    InsertSceneToBuildSettings(BootstrapSettings.Singleton.PostBootstrapScenePath);
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if(BootstrapSettings.Singleton.EnableEditorBootstrapping && BootstrapSettings.Singleton.EditorPostBootstrapHandlingType == EditorPostBootstrapHandlingType.LoadPostBootstrapScene)
            {
                if(string.IsNullOrEmpty(BootstrapSettings.Singleton.EditorPostBootstrapScenePath))
                {
                    EditorGUILayout.HelpBox("EDITOR Post-bootstrap scene is not assigned.", MessageType.Error);
                }
                else
                {
                    if (!DoesSceneExist(BootstrapSettings.Singleton.EditorPostBootstrapScenePath))
                    {
                        EditorGUILayout.HelpBox("Assigned EDITOR post-bootstrap scene does not exist.", MessageType.Error);
                    }
                    else if (!IsSceneInBuildSettings(BootstrapSettings.Singleton.EditorPostBootstrapScenePath))
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.HelpBox("Assigned EDITOR post-bootstrap scene is not in build settings.", MessageType.Warning);
                        
                        if (GUILayout.Button("Fix", GUILayout.Width(50f), GUILayout.Height(EditorGUIUtility.singleLineHeight * 2f)))
                        {
                            InsertSceneToBuildSettings(BootstrapSettings.Singleton.EditorPostBootstrapScenePath);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
        }


        private void DrawSectionGeneral()
        {
            EditorGUILayout.LabelField("GENERAL:", _titleLabelStyle, GUILayout.Height(30f));

            Color cachedColor = GUI.backgroundColor;
            
            if (BootstrapSettings.Singleton.BootstrapScenePaths == null || BootstrapSettings.Singleton.BootstrapScenePaths.Count == 0)
                GUI.backgroundColor = Color.red;
            
            _objectSo.Update();
            _scenesRl!.DoLayoutList();
            _objectSo.ApplyModifiedProperties();
            
            GUI.backgroundColor = cachedColor;
        }


        private void DrawSectionBuild()
        {
            EditorGUILayout.LabelField("BUILD:", _titleLabelStyle, GUILayout.Height(30f));

            BootstrapSettings.Singleton.BuildPostBootstrapHandlingType = (BuildPostBootstrapHandlingType)EditorGUILayout.Popup(
                new GUIContent(
                    "After bootstrapping:",
                    "Decides what happens after all bootstrapping is done, when in a built project.\n\nDo nothing:\n- Stay in the last loaded bootstrap scene.\n\nLoad post-bootstrap scene:\n- Load the scene specified in the 'Post-bootstrap scene' field (usually your gameplay scene)."),
                (int)BootstrapSettings.Singleton.BuildPostBootstrapHandlingType, new[]
                {
                    "Do nothing",
                    "Load post-bootstrap scene"
                });
            
            bool shouldShowPostSceneField = BootstrapSettings.Singleton.BuildPostBootstrapHandlingType == BuildPostBootstrapHandlingType.LoadPostBootstrapScene;

            if (shouldShowPostSceneField)
            {
                SceneAsset newScene = DrawSceneAssetFieldEditorGuiLayout(BootstrapSettings.Singleton.PostBootstrapScenePath, "Post-bootstrap scene");
                BootstrapSettings.Singleton.PostBootstrapScenePath = AssetDatabase.GetAssetPath(newScene);
            }
        }


        private void DrawSectionEditor()
        {
            EditorGUILayout.LabelField("EDITOR:", _titleLabelStyle, GUILayout.Height(30f));

            float cachedLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 190f;
            BootstrapSettings.Singleton.EnableEditorBootstrapping = EditorGUILayout.Toggle("Enable bootstrapping in editor?", BootstrapSettings.Singleton.EnableEditorBootstrapping);
            
            if (!BootstrapSettings.Singleton.EnableEditorBootstrapping)
                return;

            BootstrapSettings.Singleton.EditorPostBootstrapHandlingType = (EditorPostBootstrapHandlingType)EditorGUILayout.Popup(
                new GUIContent(
                    "After bootstrapping:",
                    "Decides what happens after all bootstrapping is done, when in editor.\n\nDo nothing:\n- Stay in the last loaded bootstrap scene.\n\nLoad back to current scene:\n- Load back to the scene that was active before bootstrapping.\n\nLoad post-bootstrap scene:\n- Load the scene specified in the 'Post-bootstrap scene' field."),
                (int)BootstrapSettings.Singleton.EditorPostBootstrapHandlingType, new[]
                {
                    "Do nothing",
                    "Load back to current scene",
                    "Load post-bootstrap scene"
                });
            
            bool shouldShowPostSceneField = BootstrapSettings.Singleton.EditorPostBootstrapHandlingType == EditorPostBootstrapHandlingType.LoadPostBootstrapScene;

            if (shouldShowPostSceneField)
            {
                SceneAsset newScene = DrawSceneAssetFieldEditorGuiLayout(BootstrapSettings.Singleton.EditorPostBootstrapScenePath, "Post-bootstrap scene");
                BootstrapSettings.Singleton.EditorPostBootstrapScenePath = AssetDatabase.GetAssetPath(newScene);
            }
            
            EditorGUIUtility.labelWidth = cachedLabelWidth;
        }


        private void DrawSectionDebug()
        {
            float cachedLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 190f;
            EditorGUILayout.LabelField("DEBUGGING:", _titleLabelStyle, GUILayout.Height(30f));

            BootstrapSettings.Singleton.EnableVerboseLogging = EditorGUILayout.Toggle("Enable verbose logging?", BootstrapSettings.Singleton.EnableVerboseLogging);
            
            EditorGUIUtility.labelWidth = cachedLabelWidth;
        }


        private void DrawSceneAssetFieldForList(int sceneIndex, Rect rect)
        {
            float x = rect.x;
            float y = rect.y;

            y += EditorGUIUtility.standardVerticalSpacing;
            
            string oldPath = _scenesRl.serializedProperty.GetArrayElementAtIndex(sceneIndex).stringValue;
            
            SceneAsset oldScene = AssetDatabase.LoadAssetAtPath(oldPath, typeof(SceneAsset)) as SceneAsset;

            x += 20f;

            Color cachedColor = GUI.backgroundColor;
            if (oldScene == null || oldPath == BootstrapSettings.Singleton.PostBootstrapScenePath || oldPath == BootstrapSettings.Singleton.EditorPostBootstrapScenePath)
            {
                // Draw an object field for a scene asset with red background.
                GUI.backgroundColor = Color.red;
            }
            
            SceneAsset newScene = EditorGUI.ObjectField(new Rect(x, y, 200f, EditorGUIUtility.singleLineHeight), oldScene, typeof(SceneAsset), false) as SceneAsset;
            string newPath = newScene == null ? null : AssetDatabase.GetAssetPath(newScene);
            _scenesRl.serializedProperty.GetArrayElementAtIndex(sceneIndex).stringValue = newPath;
            
            GUI.backgroundColor = cachedColor;
        }


        private static SceneAsset DrawSceneAssetFieldEditorGuiLayout(string scenePath, string label)
        {
            SceneAsset oldScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            Color cachedColor = GUI.backgroundColor;
            if (oldScene == null || BootstrapSettings.Singleton.BootstrapScenePaths.Contains(scenePath))
            {
                // Draw an object field for a scene asset with red background.
                GUI.backgroundColor = Color.red;
            }
            
            SceneAsset newScene = EditorGUILayout.ObjectField(label, oldScene, typeof(SceneAsset), false) as SceneAsset;
            
            GUI.backgroundColor = cachedColor;

            return newScene;
        }


        private static int GetSceneBuildSettingsIndex(string scenePath)
        {
            EditorBuildSettingsScene[] buildSettingsScenes = EditorBuildSettings.scenes;
            
            return Array.FindIndex(buildSettingsScenes, s => s.path == scenePath);
        }


        private static void InsertSceneToBuildSettings(string scenePath, int index = -1, bool log = true)
        {
            EditorBuildSettings.scenes ??= Array.Empty<EditorBuildSettingsScene>();
            
            if (EditorBuildSettings.scenes.Any(editorBuildSettingsScene => editorBuildSettingsScene.path == scenePath))
                return;

            EditorBuildSettingsScene[] currentScenes = EditorBuildSettings.scenes;

            EditorBuildSettingsScene[] newScenes;

            if (index >= 0)
            {
                newScenes = new EditorBuildSettingsScene[currentScenes.Length + 1];
                for (int i = 0; i < newScenes.Length; i++)
                {
                    if (i < index)
                        newScenes[i] = currentScenes[i];
                    else if (i == index)
                        newScenes[i] = new EditorBuildSettingsScene(scenePath, true);
                    else
                        newScenes[i] = currentScenes[i - 1];
                }
            }
            else
            {
                newScenes = new EditorBuildSettingsScene[currentScenes.Length + 1];
                for (int i = 0; i < currentScenes.Length; i++)
                    newScenes[i] = currentScenes[i];

                newScenes[^1] = new EditorBuildSettingsScene(scenePath, true);
            }

            EditorBuildSettings.scenes = newScenes;
            
            if(log)
                EasyBootstrapLogger.Log($"Added scene {scenePath} to build settings!");
        }


        private static void ChangeSceneBuildSettingsIndex(int currentIndex, int newIndex)
        {
            EditorBuildSettingsScene[] buildSettingsScenes = EditorBuildSettings.scenes;
            
            EditorBuildSettingsScene scene = buildSettingsScenes[currentIndex];
            string scenePath = scene.path;
            RemoveSceneFromBuildSettings(currentIndex);
            InsertSceneToBuildSettings(scenePath, newIndex, false);
            
            EasyBootstrapLogger.Log($"Moved scene {scenePath} in build settings from index {currentIndex} to {newIndex}.");
        }


        private static void RemoveSceneFromBuildSettings(int sceneIndex)
        {
            EditorBuildSettingsScene[] currentScenes = EditorBuildSettings.scenes;
            EditorBuildSettingsScene[] newScenes = new EditorBuildSettingsScene[currentScenes.Length - 1];
            
            int newIndex = 0;
            for (int i = 0; i < currentScenes.Length; i++)
            {
                if (i == sceneIndex)
                    continue;

                newScenes[newIndex] = currentScenes[i];
                newIndex++;
            }

            EditorBuildSettings.scenes = newScenes;
            
            EasyBootstrapLogger.LogVerbose($"Removed scene at index {sceneIndex} from build settings!");
        }
        
        
        private static bool IsSceneInBuildSettings(string scenePath)
        {
            return EditorBuildSettings.scenes.Any(editorBuildSettingsScene => editorBuildSettingsScene.path == scenePath);
        }


        private static bool DoesSceneExist(string scenePath)
        {
            return AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) != null;
        }


        private static void DrawSpacerLine(int thickness = 2, int verticalPadding = 10, int horizontalPadding = 20)
        {
            Color color = new(0.16f, 0.16f, 0.16f);
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(verticalPadding + thickness));
            r.height = thickness;
            r.y += verticalPadding / 2f;
            r.x -= 2;
            r.x += horizontalPadding / 2f;
            r.width -= horizontalPadding;
            EditorGUI.DrawRect(r, color);
        }
    }
}