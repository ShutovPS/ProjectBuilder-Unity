using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Mobcast.Coffee.Build.Editor
{
    /// <summary>
    /// プロジェクトビルダーエディタ.
    /// インスペクタをオーバーライドして、ビルド設定エディタを構成します.
    /// エディタから直接ビルドパイプラインを実行できます.
    /// </summary>
    internal class ProjectBuilderEditor : EditorWindow
    {
        protected const string kPrefsKeyLastSelected = "ProjectBuilderEditor_LastSelected";
        
        
        protected static GUIContent contentOpen = null;
        protected static GUIContent contentTitle = new GUIContent();
        protected static ReorderableList roSceneList = null;
        protected static ReorderableList roExcludeDirectoriesList = null;
        protected static ReorderableList roBuilderList = null;

        protected static GUIStyle styleCommand = null;
        protected static GUIStyle styleSymbols = null;
        protected static GUIStyle styleTitle = null;

        protected static string s_EndBasePropertyName = "";
        protected static string[] s_AvailableScenes = null;
        protected static List<ProjectBuilder> s_BuildersInProject = null;
        
        
        protected Vector2 _scrollPosition = Vector2.zero;
        protected ProjectBuilder[] _targets = null;
        protected SerializedObject _serializedObject = null;

        
        protected static readonly Dictionary<BuildTarget, IBuildSettings> s_BuildTargetSettings =
            typeof(ProjectBuilder).Assembly
                .GetTypes()
                .Where(x => x.IsPublic && !x.IsInterface &&
                    typeof(IBuildSettings).IsAssignableFrom(x))
                .Select(x => Activator.CreateInstance(x) as IBuildSettings)
                .OrderBy(x => x.GetBuildTarget)
                .ToDictionary(x => x.GetBuildTarget);

        protected static readonly int[] s_BuildTargetValues = s_BuildTargetSettings.Keys.Cast<int>().ToArray();

        protected static readonly GUIContent[] s_BuildTargetLabels =
            s_BuildTargetSettings.Keys.Select(x => new GUIContent(x.ToString())).ToArray();

        public static Texture GetBuildTargetIcon(IProjectBuilder builder)
        {
            return builder.BuildApplication && s_BuildTargetSettings.ContainsKey(builder.ActualBuildTarget)
                ? s_BuildTargetSettings[builder.ActualBuildTarget].GetIcon
                : EditorGUIUtility.FindTexture("BuildSettings.Editor.Small");
        }

        
        [MenuItem("Project Builder/Builder %#w")]
        public static void OnOpenFromMenu()
        {
            EditorWindow.GetWindow<ProjectBuilderEditor>("Project Builder");
        }

        protected virtual void Initialize()
        {
            if (styleCommand != null)
            {
                return;
            }

            styleTitle = new GUIStyle("IN BigTitle");
            styleTitle.alignment = TextAnchor.UpperLeft;
            styleTitle.fontSize = 12;
            styleTitle.stretchWidth = true;
            styleTitle.margin = new RectOffset();

            styleSymbols = new GUIStyle(EditorStyles.textArea);
            styleSymbols.wordWrap = true;

            styleCommand = new GUIStyle(EditorStyles.textArea);
            styleCommand.stretchWidth = false;
            styleCommand.fontSize = 9;
            contentOpen = new GUIContent(EditorGUIUtility.FindTexture("project"));

            // Find end property in ProjectBuilder.
            var dummy = ScriptableObject.CreateInstance<ProjectBuilder>();
            var sp = new SerializedObject(dummy).GetIterator();

            sp.Next(true);

            while (sp.Next(false))
            {
                s_EndBasePropertyName = sp.name;
            }

            // Scene list.
            roSceneList = new ReorderableList(new List<ProjectBuilder.SceneSetting>(),
                typeof(ProjectBuilder.SceneSetting));
            roSceneList.drawElementCallback += DrawScenesList;

            roSceneList.headerHeight = 0;
            roSceneList.elementHeight = 18;

            // Exclude Directories List
            roExcludeDirectoriesList = new ReorderableList(new List<string>(), typeof(string));
            roExcludeDirectoriesList.drawElementCallback += DrawExcludeDirectoriesList;

            roExcludeDirectoriesList.headerHeight = 0;
            roExcludeDirectoriesList.elementHeight = 18;

            // Builder list.
            roBuilderList = new ReorderableList(s_BuildersInProject, typeof(ProjectBuilder));
            roBuilderList.onSelectCallback = (list) => Selection.activeObject = list.list[list.index] as ProjectBuilder;

            roBuilderList.onAddCallback += OnAddBuilderItem;

            roBuilderList.onRemoveCallback += OnRemoveBuilderItem;

            roBuilderList.drawElementCallback += DrawBuildersList;

            roBuilderList.headerHeight = 0;
            roBuilderList.draggable = false;

            contentTitle =
                new GUIContent(ProjectBuilderUtil.GetAssets<Texture2D>(typeof(ProjectBuilder).Name + " Icon").FirstOrDefault());

            DestroyImmediate(dummy);
        }
        //---- ▲ GUIキャッシュ ▲ ----

        //-------------------------------
        //	Unityコールバック.
        //-------------------------------
        /// <summary>
        /// Raises the enable event.
        /// </summary>
        protected virtual void OnEnable()
        {
            _targets = null;

            // 最後に選択したビルダーが存在する.
            string path =
                AssetDatabase.GUIDToAssetPath(
                    PlayerPrefs.GetString(kPrefsKeyLastSelected + EditorUserBuildSettings.activeBuildTarget));

            if (!string.IsNullOrEmpty(path))
            {
                var builder = AssetDatabase.LoadAssetAtPath<ProjectBuilder>(path);
                if (builder)
                {
                    SelectBuilder(new[] {builder});
                }
            }

            if (_targets == null)
            {
                // 選択しているオブジェクト内にビルダーが存在する
                if (Selection.objects.OfType<ProjectBuilder>().Any())
                {
                    SelectBuilder(Selection.objects.OfType<ProjectBuilder>().ToArray());
                }
                else
                {
                    // プロジェクト内にビルダーが存在する
                    var builders = ProjectBuilderUtil.GetAssets<ProjectBuilder>();

                    if (builders.Any())
                    {
                        SelectBuilder(builders.Take(1).ToArray());
                    }
                }
            }

            Selection.selectionChanged += OnSelectionChanged;
            minSize = new Vector2(300, 300);
        }

        /// <summary>
        /// Raises the disable event.
        /// </summary>
        protected virtual void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }

        protected virtual void SelectBuilder(ProjectBuilder[] builders)
        {
            // Get all scenes in build from BuildSettings.
            s_AvailableScenes = EditorBuildSettings.scenes.Select(x => Path.GetFileName(x.path)).ToArray();

            // Get all builder assets in project.
            s_BuildersInProject = new List<ProjectBuilder>(
                ProjectBuilderUtil.GetAssets<ProjectBuilder>()
                    .OrderBy(b => b.BuildApplication)
                    .ThenBy(b => b.ActualBuildTarget)
            );

            _targets = 0 < builders.Length
                ? builders
                : s_BuildersInProject.Take(1).ToArray();

            _serializedObject = null;

            contentTitle.text = 0 < _targets.Length
                ? _targets.Select(x => "  " + x.name).Aggregate((a, b) => a + "\n" + b)
                : "";

            // 最後に選択したビルダーアセットを記憶.
            var lastSelected = _targets.FirstOrDefault(x => x.ActualBuildTarget == EditorUserBuildSettings.activeBuildTarget);

            if (lastSelected)
            {
                PlayerPrefs.SetString(kPrefsKeyLastSelected + EditorUserBuildSettings.activeBuildTarget,
                    AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(lastSelected)));
                PlayerPrefs.Save();
            }
        }

        protected virtual void OnSelectionChanged()
        {
            var builders = Selection.objects.OfType<ProjectBuilder>().ToArray();

            if (builders.Length > 0 || _targets.Any(x => !x))
            {
                SelectBuilder(builders);
                Repaint();
            }
        }

        protected virtual void OnGUI()
        {
            Initialize();

            if (_targets == null || _targets.Length == 0)
            {
                if (GUILayout.Button("Create New ProjectBuilder Asset"))
                {
                    Selection.activeObject = ProjectBuilderUtil.CreateBuilderAsset();
                }

                return;
            }

            using (var svs = new EditorGUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = svs.scrollPosition;

                _serializedObject = _serializedObject ?? new SerializedObject(_targets);
                _serializedObject.Update();

                GUILayout.Label(contentTitle, styleTitle);

                DrawControlPanel();
                
                DrawApplicationBuildSettings();
                DrawAssetBundleBuildSettings();
                
                DrawBuildTragetSettings();

                _serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Draw application build settings.
        /// </summary>
        protected virtual void DrawApplicationBuildSettings()
        {
            var spBuildApplication = _serializedObject.FindProperty("_buildApplication");
            using (new EditorGUIExtensions.GroupScope("Application Build Setting"))
            {
                EditorGUILayout.PropertyField(spBuildApplication);
                if (spBuildApplication.boolValue)
                {
                    DrawBasicBuildSettings();
                    
                    DrawVersionBuildSettings();

                    DrawOutputBuildSettings();
                    
                    DrawAdvancedBuildSettings();
                }
            }
        }

        /// <summary>
        /// Draw Basic settings.
        /// </summary>
        protected virtual void DrawBasicBuildSettings()
        {
            var spBuildTarget = _serializedObject.FindProperty("_buildTarget");
            
            // Basic Options
            EditorGUILayout.IntPopup(spBuildTarget, s_BuildTargetLabels, s_BuildTargetValues);
            EditorGUILayout.PropertyField(_serializedObject.FindProperty("_companyName"));
            EditorGUILayout.PropertyField(_serializedObject.FindProperty("_productName"));
            EditorGUILayout.PropertyField(_serializedObject.FindProperty("_applicationIdentifier"));
        }

        /// <summary>
        /// Draw version settings.
        /// </summary>
        protected virtual void DrawVersionBuildSettings()
        {
            var spBuildTarget = _serializedObject.FindProperty("_buildTarget");
            
            GUILayout.Space(8);

            // Version.
            EditorGUILayout.LabelField("Version Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            var version = _serializedObject.FindProperty("_version");
            string versionString = version.stringValue;
            EditorGUILayout.PropertyField(version);
            if (!Regex.IsMatch(version.stringValue, @"^\d\d?.\d\d?.\d\d?$"))
            {
                version.stringValue = versionString;
            }

            GUILayout.BeginHorizontal();

            GUIContent versionCodeContent = new GUIContent();

            // Internal version for the build target.
            switch ((BuildTarget)spBuildTarget.intValue)
            {
                case BuildTarget.Android:
                    versionCodeContent.text = "Version Code";
                    break;

                case BuildTarget.iOS:
                    versionCodeContent.text = "Build Number";
                    break;

                case BuildTarget.WebGL:
                    versionCodeContent.text = "Build Number";
                    break;
            }

            var versionCode = _serializedObject.FindProperty("_versionCode");
            EditorGUILayout.PropertyField(versionCode, versionCodeContent);

            EditorGUI.BeginDisabledGroup(versionCode.intValue <= 0);
            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                versionCode.intValue--;
            }

            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(versionCode.intValue >= 99);
            if (GUILayout.Button("+", GUILayout.Width(20)))
            {
                versionCode.intValue++;
            }

            EditorGUI.EndDisabledGroup();
            versionCode.intValue = Mathf.Clamp(versionCode.intValue, 0, 99);

            GUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Draw output settings.
        /// </summary>
        protected virtual void DrawOutputBuildSettings()
        {
            GUILayout.Space(8);
            
            EditorGUILayout.LabelField("Output Options", EditorStyles.boldLabel);
            
            EditorGUI.indentLevel++;
            // Open output.
            var buildsName = _serializedObject.FindProperty("_buildsName");
            EditorGUILayout.PropertyField(buildsName);
            var buildsDirectoryName = _serializedObject.FindProperty("_buildsDirectoryName");
            EditorGUILayout.PropertyField(buildsDirectoryName);
                    
            var rect = EditorGUILayout.GetControlRect(true);
            var buildsPath = _serializedObject.FindProperty("_buildsPath");
            EditorGUIExtensions.DirectoryPathField(rect, buildsPath, new GUIContent("Builds Path"),
                "Select builds output directory.");
            EditorGUILayout.PropertyField(_serializedObject.FindProperty("_openBuildPathAfterBuild"));
            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Draw Advanced settings.
        /// </summary>
        protected virtual void DrawAdvancedBuildSettings()
        {
            // Advanced Options
            GUILayout.Space(8);
            
            EditorGUILayout.LabelField("Advanced Options", EditorStyles.boldLabel);
            
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(_serializedObject.FindProperty("_developmentBuild"));

            DrawScriptingDefineSymbolsBuildSettings();
                    
            DrawScenesBuildSettings();

            DrawExcludeDirectoriesBuildSettings();

            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Draw Scripting Define Symbols settings.
        /// </summary>
        protected virtual void DrawScriptingDefineSymbolsBuildSettings()
        {
            EditorGUILayout.LabelField("Scripting Define Symbols");
            GUILayout.Space(-18);
            EditorGUILayout.PropertyField(_serializedObject.FindProperty("_defineSymbols"), GUIContent.none);
        }

        /// <summary>
        /// Draw Scenes settings.
        /// </summary>
        protected virtual void DrawScenesBuildSettings()
        {
            // Scenes In Build.
            EditorGUILayout.LabelField("Enable/Disable Scenes In Build");
            roSceneList.serializedProperty = _serializedObject.FindProperty("_scenes");

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(16);
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUI.indentLevel--;
                    roSceneList.DoLayoutList();
                    EditorGUI.indentLevel++;
                }
            }
        }

        /// <summary>
        /// Draw Exclude Directories settings.
        /// </summary>
        protected virtual void DrawExcludeDirectoriesBuildSettings()
        {
            // Exclude Directories.
            EditorGUILayout.LabelField("Exclude Directories");
            roExcludeDirectoriesList.serializedProperty = _serializedObject.FindProperty("_excludeDirectories");

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(16);
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUI.indentLevel--;
                    roExcludeDirectoriesList.DoLayoutList();
                    EditorGUI.indentLevel++;
                }
            }
        }

        /// <summary>
        /// Draw asset bundle build settings.
        /// </summary>
        protected virtual void DrawAssetBundleBuildSettings()
        {
            // AssetBundle building.
            using (new EditorGUIExtensions.GroupScope("AssetBundle Build Setting"))
            {
                var spBuildAssetBundle = _serializedObject.FindProperty("_buildAssetBundle");
                EditorGUILayout.PropertyField(spBuildAssetBundle);

                if (spBuildAssetBundle.boolValue)
                {
                    EditorGUILayout.PropertyField(_serializedObject.FindProperty("_bundleOptions"));
                    EditorGUILayout.PropertyField(_serializedObject.FindProperty("_copyToStreamingAssets"));
                }
            }
        }

        /// <summary>
        /// ターゲットごとのビルド設定を描画します.
        /// Draw build target settings.
        /// </summary>
        protected virtual void DrawBuildTragetSettings()
        {
            var spBuildApplication = _serializedObject.FindProperty("_buildApplication");
            var spBuildTarget = _serializedObject.FindProperty("_buildTarget");
            var buildTarget = (BuildTarget)spBuildTarget.intValue;

            if (spBuildApplication.boolValue && s_BuildTargetSettings.ContainsKey(buildTarget))
            {
                var builder = _serializedObject.targetObject as ProjectBuilder;
                
                string propertyKey = null;
                IBuildSettings buildSettings = null;

                switch (buildTarget)
                {
                    case BuildTarget.Android:
                        propertyKey = "_androidSettings";
                        buildSettings = builder.GetAndroidSettings;
                        break;
                    case BuildTarget.iOS:
                        propertyKey = "_iosSettings";
                        buildSettings = builder.GetiOSSettings;
                        break;
                    case BuildTarget.WebGL:
                        propertyKey = "_webGlSettings";
                        buildSettings = builder.GetWebGLSettings;
                        break;
                }
                
                if (!string.IsNullOrEmpty(propertyKey))
                {
                    
                    var settings = _serializedObject.FindProperty(propertyKey);
                    // var target = settings.serializedObject as IiOSBuildSettings;
                    // var target = s_BuildTargetSettings[buildTarget];

                    buildSettings.DrawSetting(settings);
                }
            }
        }

        /// <summary>
        /// アセットバンドルビルド設定を描画します.
        /// Control panel for builder.
        /// </summary>
        protected virtual void DrawControlPanel()
        {
            var builder = _serializedObject.targetObject as ProjectBuilder;

            GUILayout.FlexibleSpace();

            using (new EditorGUILayout.VerticalScope("box"))
            {
                DrawBuilderInfo(builder);

                DrawBuilderSettings(builder);

                //ビルドターゲットが同じ場合のみビルド可能.
                bool isTheSameBuildTarget = builder.ActualBuildTarget == EditorUserBuildSettings.activeBuildTarget;

                EditorGUI.BeginDisabledGroup(isTheSameBuildTarget);
                
                DrawSwitchBuildTarget(builder);
                
                EditorGUI.EndDisabledGroup();
                
                EditorGUI.BeginDisabledGroup(!isTheSameBuildTarget);

                DrawBuildAssetBundles(builder);

                DrawBuild(builder);
                DrawBuildAndPlay(builder);
                
                EditorGUI.EndDisabledGroup();

                // Convert to JSON.
                if (GUILayout.Button("Convert to JSON (console log)"))
                {
                    UnityEngine.Debug.Log(JsonUtility.ToJson(builder, true));
                }

                // Available builders.
                GUILayout.Space(10);
                GUILayout.Label("Available Project Builders", EditorStyles.boldLabel);
                roBuilderList.list = s_BuildersInProject;
                roBuilderList.index = s_BuildersInProject.FindIndex(x => x == _serializedObject.targetObject);
                roBuilderList.DoLayoutList();
            }
        }

        protected virtual void DrawBuilderInfo(IProjectBuilder builder)
        {
            if (builder.BuildApplication)
            {
                string productName = builder.ProductName;
                string version = builder.Version;
                string fullVersionCode = BuildPathUtils.GetVersionCodeLong(builder);

                string titleText = string.Format("{0} ver.{1} ({2})", productName, version, fullVersionCode);

                GUILayout.Label(
                    new GUIContent(titleText, GetBuildTargetIcon(builder)), EditorStyles.largeLabel);
            }
            else if (builder.BuildAssetBundle)
            {
                var assetBundles = AssetDatabase.GetAllAssetBundleNames();
                int assetBundlesCount = assetBundles.Length;

                string titleText = string.Format("{0} AssetBundles", assetBundlesCount);

                GUILayout.Label(
                    new GUIContent(titleText, GetBuildTargetIcon(builder)), EditorStyles.largeLabel);
            }
        }

        protected virtual void DrawBuilderSettings(IProjectBuilder builder)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                // Apply settings from current builder asset.
                if (GUILayout.Button(new GUIContent("Apply Setting", EditorGUIUtility.FindTexture("d_SaveAs"))))
                {
                    builder.DefineSymbol();
                    builder.ApplySettings();
                }
                
                // Read settings from project settings.
                if (GUILayout.Button(new GUIContent("Read Setting", EditorGUIUtility.FindTexture("Loading"))))
                {
                    builder.ReadSettings();
                }
                
                // Open PlayerSettings.
                if (GUILayout.Button(
                    new GUIContent("Player Setting", EditorGUIUtility.FindTexture("d_editicon.sml")), 
                    GUILayout.Width(110)))
                {
#if UNITY_2018_1_OR_NEWER
                    //						Selection.activeObject = Unsupported.GetSerializedAssetInterfaceSingleton("PlayerSettings");
                    SettingsService.OpenProjectSettings("Project/Player");
#else
                    EditorApplication.ExecuteMenuItem("Edit/Project Settings/Player");
#endif
                }
            }
        }

        protected virtual void DrawBuildAssetBundles(ProjectBuilder builder)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginDisabledGroup(!builder.BuildAssetBundle);
                // Build.
                if (GUILayout.Button(
                    new GUIContent("Build AssetBundles",
                        EditorGUIUtility.FindTexture("buildsettings.editor.small")), "LargeButton"))
                {
                    EditorApplication.delayCall += () => ProjectBuilderUtil.StartBuild(builder, false, true);
                }

                // Open output.
                var rect = EditorGUILayout.GetControlRect(false, GUILayout.Width(15));

                if (GUI.Button(new Rect(rect.x - 2, rect.y + 5, 20, 20), contentOpen, EditorStyles.label))
                {
                    Directory.CreateDirectory(builder.BundleOutputPath);
                    ProjectBuilderUtil.RevealOutputInFinder(builder.BundleOutputPath);
                }

                EditorGUI.EndDisabledGroup();
            }
        }

        protected virtual void DrawBuild(ProjectBuilder builder)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginDisabledGroup(!builder.BuildApplication);

                // Build.
                if (GUILayout.Button(
                    new GUIContent("Build",
                        EditorGUIUtility.FindTexture("preAudioPlayOff")), "LargeButton"))
                {
                    EditorApplication.delayCall += () => ProjectBuilderUtil.StartBuild(builder, false, false);
                }

                // Open output.
                var r = EditorGUILayout.GetControlRect(false, GUILayout.Width(15));

                if (GUI.Button(new Rect(r.x - 2, r.y + 5, 20, 20), contentOpen, EditorStyles.label))
                {
                    string outputFullPath = BuildPathUtils.GetOutputPath(builder);
                    ProjectBuilderUtil.RevealOutputInFinder(outputFullPath);
                }

                EditorGUI.EndDisabledGroup();
            }
        }

        protected virtual void DrawBuildAndPlay(ProjectBuilder builder)
        {
            if (GUILayout.Button(new GUIContent("Build & Run", EditorGUIUtility.FindTexture("preAudioPlayOn")),
                "LargeButton"))
            {
                EditorApplication.delayCall += () => ProjectBuilderUtil.StartBuild(builder, true, false);
            }
        }

        protected virtual void DrawSwitchBuildTarget(ProjectBuilder builder)
        {
            if (GUILayout.Button(new GUIContent("Switch Platform", EditorGUIUtility.FindTexture("d_preAudioLoopOff")),
                "LargeButton"))
            {
                EditorApplication.delayCall += () => ProjectBuilderUtil.SwitchActiveBuildTarget(builder);
            }
        }
        
        protected virtual void DrawScenesList(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = roSceneList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, 16, rect.height - 2),
                element.FindPropertyRelative("enable"), GUIContent.none);
            EditorGUIExtensions.TextFieldWithTemplate(new Rect(rect.x + 16, rect.y, rect.width - 16, rect.height - 2),
                element.FindPropertyRelative("name"), GUIContent.none,
                s_AvailableScenes, false);
        }
        
        protected virtual void DrawExcludeDirectoriesList(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = roExcludeDirectoriesList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUIExtensions.DirectoryPathField(rect, element, GUIContent.none, "Select exclude directory in build.");
        }
        
        protected virtual void DrawBuildersList(Rect rect, int index, bool isActive, bool isFocused)
        {
            var builder = roBuilderList.list[index] as ProjectBuilder; //オブジェクト取得.
            if (!builder)
            {
                return;
            }

            GUI.DrawTexture(new Rect(rect.x, rect.y + 2, 16, 16), GetBuildTargetIcon(builder));
            GUI.Label(new Rect(rect.x + 16, rect.y + 2, rect.width - 16, rect.height - 2),
                new GUIContent(string.Format("{0} ({1})", builder.name, builder.ProductName)));
        }
            
        protected virtual void OnAddBuilderItem(ReorderableList list)
        {
            EditorApplication.delayCall += () =>
            {
                ProjectBuilderUtil.CreateBuilderAsset();
                OnSelectionChanged();
            };
        }
            
        protected virtual void OnRemoveBuilderItem(ReorderableList list)
        {
            EditorApplication.delayCall += () =>
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(list.list[list.index] as ProjectBuilder));
                AssetDatabase.Refresh();
                OnSelectionChanged();
            };
        }
    }
}
