using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System;
using System.Reflection;

namespace Mobcast.Coffee.Build.Editor
{
    /// <summary>
    /// BrojectBuilder共用クラス.
    /// </summary>
    internal class ProjectBuilderUtil : ScriptableSingleton<ProjectBuilderUtil>
    {
        public const string OPT_BUILDER = "-builder";
        public const string OPT_CLOUD_BUILDER = "-bvrbuildtarget";
        public const string OPT_APPEND_SYMBOL = "-appendSymbols";

        public const string OPT_OVERRIDE = "-override";

        //		public const string OPT_NO_BUILD = "-noBuild";
        //		public const string OPT_RESUME = "-resume";
        public const string OPT_DEV_BUILD_NUM = "-devBuildNumber";

        
        /// <summary>実行時オプション引数.</summary>
        public static readonly Dictionary<string, string> _executeArguments = new Dictionary<string, string>();

        /// <summary>現在のプロジェクトディレクトリ.</summary>
        public static readonly string projectDir = Environment.CurrentDirectory.Replace('\\', '/');

        /// <summary>出力バージョンファイルパス.ビルド成功時に、バンドルバージョンを出力します.</summary>
        //		public static readonly string buildVersionPath = Path.Combine(projectDir, "BUILD_VERSION");

        //		/// <summary>開発ビルド番号パス.ビルド成功時に値をインクリメントします.</summary>
        //		public static readonly string developBuildNumberPath = Path.Combine(projectDir, "DEVELOP_BUILD_NUMBER");

        /// <summary>現在のプロジェクトで利用されるビルダークラス.</summary>
        public static readonly Type builderType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .FirstOrDefault(x => x.IsSubclassOf(typeof(ProjectBuilder)))
            ?? typeof(ProjectBuilder);

        public static readonly MethodInfo miSetIconForObject =
            typeof(EditorGUIUtility).GetMethod("SetIconForObject", BindingFlags.Static | BindingFlags.NonPublic);


        [SerializeField] protected ProjectBuilder _currentBuilder;

        /// <summary>On finished compile callback.</summary>
        [SerializeField] protected bool _buildAndRun = false;

        [SerializeField] protected bool _buildAssetBundle = false;
        
        
        /// <summary>
        /// 現在のビルドに利用したビルダー.
        /// </summary>
        public static ProjectBuilder currentBuilder
        {
            get { return instance._currentBuilder; }
            private set { instance._currentBuilder = value; }
        }


        /// <summary>コンパイル完了時に呼び出されるメソッド.</summary>
        [InitializeOnLoadMethod]
        private static void InitializeOnLoadMethod()
        {
            // Get command line options from arguments.
            string argKey = "";

            foreach (string arg in System.Environment.GetCommandLineArgs())
            {
                if (arg.IndexOf('-') == 0)
                {
                    argKey = arg;
                    _executeArguments[argKey] = "";
                }
                else if (0 < argKey.Length)
                {
                    _executeArguments[argKey] = arg;
                    argKey = "";
                }
            }

            // When custom builder script exist, convert all builder assets.
            EditorApplication.delayCall += UpdateBuilderAssets;
        }

        /// <summary>Update builder assets.</summary>
        private static void UpdateBuilderAssets()
        {
            var builderScript = Resources.FindObjectsOfTypeAll<MonoScript>()
                .FirstOrDefault(x => x.GetClass() == builderType);

            var icon = GetAssets<Texture2D>(typeof(ProjectBuilder).Name + " Icon")
                .FirstOrDefault();

            // 
            if (builderType == typeof(ProjectBuilder))
            {
                return;
            }

            // Set Icon
            if (icon && builderScript && miSetIconForObject != null)
            {
                miSetIconForObject.Invoke(null, new object[] {builderScript, icon});
                EditorUtility.SetDirty(builderScript);
            }

            // Update script reference for builders.
            foreach (var builder in GetAssets<ProjectBuilder>())
            {
                // Convert 'm_Script' to custom builder script.
                var so = new SerializedObject(builder);
                so.Update();
                so.FindProperty("m_Script").objectReferenceValue = builderScript;
                so.ApplyModifiedProperties();
            }

            AssetDatabase.Refresh();
        }

        /// <summary>型を指定したアセットを検索します.</summary>
        public static IEnumerable<T> GetAssets<T>(string name = "") where T : UnityEngine.Object
        {
            string filter = string.Format("t:{0} {1}", typeof(T).Name, name);

            var assets = AssetDatabase.FindAssets(filter);
            
            var result = assets.Select(
                    x => AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(x), typeof(T)) as
                        T);

            return result;
        }

        /// <summary>実行引数からビルダーを取得します.</summary>
        public static ProjectBuilder GetBuilderFromExecuteArgument()
        {
            string name;
            var args = _executeArguments;

            //UnityCloudBuild対応
            if (args.TryGetValue(ProjectBuilderUtil.OPT_CLOUD_BUILDER, out name))
            {
                name = name.Replace("-", " ");
            }
            else if (!args.TryGetValue(ProjectBuilderUtil.OPT_BUILDER, out name))
            {
                //引数にbuilderオプションが無かったらエラー.
                throw new UnityException(ProjectBuilder.kLogType +
                    "Error : You need to specify the builder as follows. '-builder <builder asset name>'");
            }

            var builder = GetAssets<ProjectBuilder>(name).FirstOrDefault();
            //ビルダーアセットが特定できなかったらエラー.
            if (!builder)
            {
                throw new UnityException(ProjectBuilder.kLogType +
                    "Error : The specified builder could not be found. " + name);
            }
            else if (builder.ActualBuildTarget != EditorUserBuildSettings.activeBuildTarget)
            {
                throw new UnityException(ProjectBuilder.kLogType +
                    "Error : The specified builder's build target is not " +
                    EditorUserBuildSettings.activeBuildTarget);
            }
            else
            {
                UnityEngine.Debug.Log(ProjectBuilder.kLogType + "Builder selected : " + builder);
            }

            // 上書き用json
            string json;
            if (args.TryGetValue(ProjectBuilderUtil.OPT_OVERRIDE, out json))
            {
                UnityEngine.Debug.Log(ProjectBuilder.kLogType + "Override builder with json as following\n" + json);
                JsonUtility.FromJsonOverwrite(json, builder);
            }

            return builder;
        }

        /// <summary>Create and save a new builder asset with current PlayerSettings.</summary>
        public static ProjectBuilder CreateBuilderAsset()
        {
            if (!Directory.Exists("Assets/Editor"))
            {
                AssetDatabase.CreateFolder("Assets", "Editor");
            }

            // Open save file dialog.
            string filename = AssetDatabase.GenerateUniqueAssetPath(
                string.Format("Assets/Editor/Default {0}.asset", EditorUserBuildSettings.activeBuildTarget));

            string path = EditorUtility.SaveFilePanelInProject("Create New Builder Asset", Path.GetFileName(filename),
                "asset", "", "Assets/Editor");
            if (path.Length == 0)
            {
                return null;
            }

            // Create and save a new builder asset.
            var builder = ScriptableObject.CreateInstance(builderType) as ProjectBuilder;

            AssetDatabase.CreateAsset(builder, path);
            AssetDatabase.SaveAssets();

            Selection.activeObject = builder;

            return builder;
        }
        
        /// <summary>
        /// パスを開きます.
        /// </summary>
        public static void RevealOutputInFinder(string path)
        {
            if (InternalEditorUtility.inBatchMode)
            {
                return;
            }

            string parent = Path.GetDirectoryName(path);

            EditorUtility.RevealInFinder(
                (Directory.Exists(path) || File.Exists(path)) ? path :
                (Directory.Exists(parent) || File.Exists(parent)) ? parent :
                projectDir
            );
        }


#region BUILDING

        /// <summary>
        /// Registers the builder.
        /// </summary>
        public static void StartBuild(ProjectBuilder builder, bool buildAndRun, bool buildAssetBundle)
        {
            currentBuilder = builder;
            instance._buildAndRun = buildAndRun;
            instance._buildAssetBundle = buildAssetBundle;

            // When script symbol has changed, resume to build after compile finished.
            if (builder.DefineSymbol())
            {
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayProgressBar("Pre Compile to Build", "", 0.9f);
                }
                
                Compile.onFinishedCompile += ResumeBuild;
            }
            else
            {
                ResumeBuild(true);
            }
        }

        /// <summary>
        /// Resumes the build.
        /// </summary>
        public static void ResumeBuild(bool compileSuccessfully)
        {
            bool success = false;

            try
            {
                if (!Application.isBatchMode)
                {
                    EditorUtility.ClearProgressBar();
                }

                if (compileSuccessfully && currentBuilder)
                {
                    currentBuilder.ApplySettings();

                    if (instance._buildAssetBundle)
                    {
                        success = currentBuilder.BuildAssetBundles();
                    }
                    else
                    {
                        success = currentBuilder.BuildPlayer(instance._buildAndRun);
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(success ? 0 : 1);
            }
        }

#endregion BUILDING

#region PLATFORMS

        /// <summary>Select a new build target to be active.</summary>
        public static void SwitchActiveBuildTarget(IProjectBuilder projectBuilder)
        {
            var target = projectBuilder.ActualBuildTarget;
            var targetGroup = projectBuilder.BuildTargetGroup;

            EditorUserBuildSettings.SwitchActiveBuildTarget(targetGroup, target);
        }

#endregion PLATFORMS
    }
}
