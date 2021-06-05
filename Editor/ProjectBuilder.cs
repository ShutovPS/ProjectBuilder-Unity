using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Text;
using UnityEditor.Build.Reporting;
using Mobcast.Coffee.Build.Editor;

namespace Mobcast.Coffee.Build
{
    /// <summary>
    /// プロジェクトのビルド設定を管理するクラスです.
    /// このクラスを継承すると、プロジェクトごとの独自処理を追加できます.
    /// </summary>
    public class ProjectBuilder : ScriptableObject, IProjectBuilder
    {
        public const string kLogType = "#### [ProjectBuilder] ";


        //-------------------------------
        //	ビルド概要.
        //-------------------------------
        /// <summary>Build Application.</summary>
        [Tooltip("Build Application.")]
        [SerializeField] protected bool _buildApplication = true;

        /// <summary>ビルドターゲットを指定します.</summary>
        [Tooltip("ビルドターゲットを指定します.")]
        [SerializeField] protected BuildTarget _buildTarget = BuildTarget.NoTarget;


        /// <summary>端末に表示されるプロダクト名を指定します.</summary>
        [Tooltip("端末に表示されるプロダクト名を指定します.")]
        [SerializeField] protected string _productName = null;

        /// <summary>会社名を指定します.</summary>
        [Tooltip("会社名を指定します.")]
        [SerializeField] protected string _companyName = null;


        /// <summary>プロダクトのバンドル識別子を指定します.</summary>
        [Tooltip("プロダクトのバンドル識別子を指定します.")]
        [SerializeField] protected string _applicationIdentifier = null;


        [SerializeField] protected string _buildsPath = "Build";
        [SerializeField] protected string _buildsDirectoryName = "$PLATFORM";
        [SerializeField] protected string _buildsName = "$IDENTIFIER_$VERSION_$VERSION_CODE_LONG$EXECUTABLE";

        [SerializeField] protected bool _openBuildPathAfterBuild = false;

        //-------------------------------
        //	バージョン設定.
        //-------------------------------
        /// <summary>アプリのバージョンを指定します.</summary>
        [Tooltip("アプリのバージョンを指定します.")]
        [SerializeField] protected string _version = "1.0.0";

        /// <summary>バンドルコードを指定します.Androidの場合はVersionCode, iOSの場合はBuildNumberに相当します.この値は、リリース毎に更新する必要があります.</summary>
        [Tooltip("整数のバージョンコードを指定します.\nAndroidの場合はVersionCode, iOSの場合はBuildNumberに相当します.\nこの値は、リリース毎に更新する必要があります.")]
        [SerializeField] protected int _versionCode = 1;

        //-------------------------------
        //	Advanced Options.
        //-------------------------------
        /// <summary>BuildOptions.Development and BuildOptions.AllowDebugging.</summary>
        [Tooltip("BuildOptions.Development and BuildOptions.AllowDebugging.")]
        [SerializeField] protected bool _developmentBuild = false;

        /// <summary>Define Script Symbols. If you have multiple definitions, separate with a semicolon(;)</summary>
        [Tooltip("Define Script Symbols.\nIf you have multiple definitions, separate with a semicolon(;)")]
        [TextArea(1, 5)]
        [SerializeField] protected string _defineSymbols = null;

        /// <summary>Enable/Disable scenes in build</summary>
        [SerializeField] protected SceneSetting[] _scenes = new SceneSetting[] { };

        /// <summary>Ignore directories in build</summary>
        [SerializeField] protected string[] _excludeDirectories = new string[] { };

        //-------------------------------
        //	AssetBundles.
        //-------------------------------
        /// <summary>Build AssetBundle.</summary>
        [Tooltip("Build AssetBundle.")]
        [SerializeField] protected bool _buildAssetBundle = false;

        /// <summary>copyToStreamingAssets.</summary>
        [Tooltip("copyToStreamingAssets.")]
        [SerializeField] protected bool _copyToStreamingAssets = false;

        /// <summary>AssetBundle options.</summary>
        [Tooltip("AssetBundle options.")]
        [SerializeField] protected BundleOptions _bundleOptions = BundleOptions.LZ4;

        //-------------------------------
        //	Build Target Settings.
        //-------------------------------
        [SerializeField] protected iOSBuildSettings _iosSettings = new iOSBuildSettings();
        [SerializeField] protected AndroidBuildSettings _androidSettings = new AndroidBuildSettings();
        [SerializeField] protected WebGLBuildSettings _webGlSettings = new WebGLBuildSettings();


        /// <summary>Build Application.</summary>
        public bool BuildApplication => _buildApplication;

        /// <summary>ビルドに利用するターゲット.</summary>
        public BuildTarget ActualBuildTarget
        {
            get { return _buildApplication ? _buildTarget : EditorUserBuildSettings.activeBuildTarget; }
        }

        /// <summary>Build target group for this builder asset.</summary>
        public BuildTargetGroup BuildTargetGroup
        {
            get { return BuildPipeline.GetBuildTargetGroup(ActualBuildTarget); }
        }

        /// <summary>プロダクトのバンドル識別子を指定します.</summary>
        public string ApplicationIdentifier => _applicationIdentifier;

        /// <summary>端末に表示されるプロダクト名を指定します.</summary>
        public string ProductName => _productName;

        /// <summary>アプリのバージョンを指定します.</summary>
        public string Version => _version;

        /// <summary>バンドルコードを指定します.Androidの場合はVersionCode, iOSの場合はBuildNumberに相当します.この値は、リリース毎に更新する必要があります.</summary>
        public int VersionCode => _versionCode;

        /// <summary>Build root path.</summary>
        public string BuildPath => _buildsPath;

        /// <summary>Build directory name.</summary>
        public string BuildDirectoryName => _buildsDirectoryName;

        /// <summary>Builds file name.</summary>
        public string BuildName => _buildsName;

        /// <summary>Build AssetBundle.</summary>
        public bool BuildAssetBundle => _buildAssetBundle;

        /// <summary>Asset Bundles output path.</summary>
        public string BundleOutputPath
        {
            get { return Path.Combine("AssetBundles", BuildPathUtils.ConvertBuildTargetToString(ActualBuildTarget)); }
        }

        public IAndroidBuildSettings GetAndroidSettings => _androidSettings;
        public IiOSBuildSettings GetiOSSettings => _iosSettings;
        public IWebGLBuildSettings GetWebGLSettings => _webGlSettings;


        //-------------------------------
        //	Unityコールバック.
        //-------------------------------
        public virtual void ReadSettings()
        {
            _buildTarget = EditorUserBuildSettings.activeBuildTarget;
            _productName = PlayerSettings.productName;
            _companyName = PlayerSettings.companyName;
#if UNITY_5_6_OR_NEWER
            _applicationIdentifier = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup);
#else
            applicationIdentifier = PlayerSettings.bundleIdentifier;
#endif
            _version = PlayerSettings.bundleVersion;
            _defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup);

            // Build target settings.
            _androidSettings.ReadSettings(this);
            _iosSettings.ReadSettings(this);
            _webGlSettings.ReadSettings(this);
        }

        //-------------------------------
        //	アクション.
        //-------------------------------
        /// <summary>
        /// Define script symbol.
        /// </summary>
        public virtual bool DefineSymbol()
        {
            var oldDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup);
            List<string> symbolList = new List<string>(_defineSymbols.Split(',', ';', '\n', '\r'));

            // Symbols specified in command line arguments.
            if (ProjectBuilderUtil._executeArguments.ContainsKey(ProjectBuilderUtil.OPT_APPEND_SYMBOL))
            {
                var argSymbols = ProjectBuilderUtil._executeArguments[ProjectBuilderUtil.OPT_APPEND_SYMBOL]
                    .Split(',', ';', '\n', '\r');

                // Include symbols.
                foreach (string s in argSymbols.Where(x => x.IndexOf("!") != 0))
                {
                    symbolList.Add(s);
                }

                // Exclude symbols start with '!'.
                foreach (string s in argSymbols.Where(x => x.IndexOf("!") == 0 && symbolList.Contains(x.Substring(1))))
                {
                    symbolList.Remove(s.Substring(1));
                }
            }

            // Update define script symbol.
            string symbols = symbolList.Count == 0 ? "" : symbolList.Aggregate((a, b) => a + ";" + b);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup, symbols);
            UnityEngine.Debug.LogFormat("{0} DefineSymbol is updated : {1} -> {2}", kLogType, oldDefineSymbols,
                symbols);

            // Any symbol has been changed?
            return oldDefineSymbols != symbols;
        }

        //-------------------------------
        //	アクション.
        //-------------------------------
        /// <summary>
        /// PlayerSettingにビルド設定を反映します.
        /// </summary>
        public void ApplySettings()
        {
            //ビルド情報を設定します.
#if UNITY_5_6_OR_NEWER
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup, _applicationIdentifier);
#else
            PlayerSettings.bundleIdentifier = applicationIdentifier;
#endif
            PlayerSettings.productName = _productName;
            PlayerSettings.companyName = _companyName;

            EditorUserBuildSettings.development = _developmentBuild;
            EditorUserBuildSettings.allowDebugging = _developmentBuild;

            //アプリバージョン.
            //実行引数に開発ビルド番号定義がある場合、ビルド番号を再定義します.
            PlayerSettings.bundleVersion = _version;
            string buildNumber;
            if (_developmentBuild &&
                ProjectBuilderUtil._executeArguments.TryGetValue(ProjectBuilderUtil.OPT_DEV_BUILD_NUM,
                    out buildNumber) &&
                !string.IsNullOrEmpty(buildNumber))
            {
                PlayerSettings.bundleVersion += "." + buildNumber;
            }

            File.WriteAllText(Path.Combine(ProjectBuilderUtil.projectDir, "BUILD_VERSION"),
                PlayerSettings.bundleVersion);

            // Scene Settings.
            var buildSettingsScenes = EditorBuildSettings.scenes;
            for (int i = 0; i < buildSettingsScenes.Length; i++)
            {
                var scene = buildSettingsScenes[i];
                var setting = _scenes.FirstOrDefault(x => x.name == Path.GetFileName(scene.path));
                if (setting != null)
                {
                    scene.enabled = setting.enable;
                }

                buildSettingsScenes[i] = scene;
            }

            EditorBuildSettings.scenes = buildSettingsScenes;

            // Build target settings.
            _iosSettings.ApplySettings(this);
            _androidSettings.ApplySettings(this);
            _webGlSettings.ApplySettings(this);

            OnApplySetting();
            AssetDatabase.SaveAssets();
        }

        //-------------------------------
        //	継承関連.
        //-------------------------------
        /// <summary>設定適用後コールバック.</summary>
        protected virtual void OnApplySetting()
        {
        }

        /// <summary>
        /// アセットバンドルをビルドします.
        /// </summary>
        /// <returns>ビルドに成功していればtrueを、それ以外はfalseを返す.</returns>
        public bool BuildAssetBundles()
        {
            try
            {
                AssetBundleManifest oldManifest = null;
                var manifestPath = Path.Combine(BundleOutputPath, ActualBuildTarget.ToString());
                if (File.Exists(manifestPath))
                {
                    var manifestAssetBundle = AssetBundle.LoadFromFile(manifestPath);
                    oldManifest = manifestAssetBundle
                        ? manifestAssetBundle.LoadAsset<AssetBundleManifest>("assetbundlemanifest")
                        : null;
                }

                UnityEngine.Debug.Log(kLogType + "BuildAssetBundles is started.");

                Directory.CreateDirectory(BundleOutputPath);
                var opt = (BuildAssetBundleOptions)_bundleOptions |
                    BuildAssetBundleOptions.DeterministicAssetBundle;
                var newManifest = BuildPipeline.BuildAssetBundles(BundleOutputPath, opt, ActualBuildTarget);

                var sb = new StringBuilder(kLogType + "AssetBundle report");
                string[] array;
                if (oldManifest)
                {
                    // 差分をログ出力.
                    var oldBundles =
                        new HashSet<string>(oldManifest ? oldManifest.GetAllAssetBundles() : new string[] { });
                    var newBundles = new HashSet<string>(newManifest.GetAllAssetBundles());

                    // 新規
                    array = newBundles.Except(oldBundles).ToArray();
                    sb.AppendFormat("\n[Added]: {0}\n", array.Length);
                    foreach (string bundleName in array)
                    {
                        sb.AppendLine("  > " + bundleName);
                    }

                    // 削除
                    array = oldBundles.Except(newBundles).ToArray();
                    sb.AppendFormat("\n[Deleted]: {0}\n", array.Length);
                    foreach (string bundleName in array)
                    {
                        sb.AppendLine("  > " + bundleName);
                    }

                    // 更新
                    array = oldBundles
                        .Intersect(newBundles)
                        .Where(x => !Hash128.Equals(oldManifest.GetAssetBundleHash(x),
                            newManifest.GetAssetBundleHash(x)))
                        .ToArray();
                    sb.AppendFormat("\n[Updated]: {0}\n", array.Length);
                    foreach (string bundleName in array)
                    {
                        sb.AppendLine("  > " + bundleName);
                    }
                }
                else
                {
                    // 新規
                    array = newManifest.GetAllAssetBundles();
                    sb.AppendFormat("\n[Added]: {0}\n", array.Length);
                    foreach (string bundleName in array)
                    {
                        sb.AppendLine("  > " + bundleName);
                    }
                }

                UnityEngine.Debug.Log(sb);
                
                if (_copyToStreamingAssets)
                {
                    string copyPath = Path.Combine(Application.streamingAssetsPath, BundleOutputPath);
                    Directory.CreateDirectory(copyPath);

                    if (Directory.Exists(copyPath))
                    {
                        FileUtil.DeleteFileOrDirectory(copyPath);
                    }

                    FileUtil.CopyFileOrDirectory(BundleOutputPath, copyPath);
                }

                UnityEngine.Debug.Log(kLogType + "BuildAssetBundles is finished successfuly.");
                return true;
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError(kLogType + "BuildAssetBundles is failed : " + e.Message);
                return false;
            }
        }

        /// <summary>
        /// BuildPipelineによるビルドを実行します.
        /// </summary>
        /// <returns>ビルドに成功していればtrueを、それ以外はfalseを返す.</returns>
        /// <param name="autoRunPlayer">Build & Runモードでビルドします.</param>
        public virtual bool BuildPlayer(bool autoRunPlayer)
        {
            if (_buildAssetBundle && !BuildAssetBundles())
            {
                return false;
            }

            if (_buildApplication)
            {
                // Exclude directories.
                ExcludeBuildDirectoriesUtil.ExcludeDirectories(_excludeDirectories);

                // Build options.
                BuildOptions opt = _developmentBuild
                    ? (BuildOptions.Development & BuildOptions.AllowDebugging)
                    : BuildOptions.None
                    | (autoRunPlayer ? BuildOptions.AutoRunPlayer : BuildOptions.None);

                // Scenes to build.
                string[] scenesToBuild = EditorBuildSettings.scenes.Where(x => x.enabled).Select(x => x.path).ToArray();
                UnityEngine.Debug.Log(kLogType + "Scenes to build : " +
                    scenesToBuild.Aggregate((a, b) => a + ", " + b));

                string outputFullPath = BuildPathUtils.GetOutputPath(this);

                if (Directory.Exists(outputFullPath))
                {
                    Directory.Delete(outputFullPath, true);
                }

                if (File.Exists(outputFullPath))
                {
                    File.Delete(outputFullPath);
                }

                // Start build.
                UnityEngine.Debug.Log(kLogType + "BuildPlayer is started. Defined symbols : " +
                    PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup));
                
                var buildReport = BuildPipeline.BuildPlayer(scenesToBuild, outputFullPath, ActualBuildTarget, opt);

                // Revert excluded directories.
                ExcludeBuildDirectoriesUtil.RevertExcludedDirectory();

                if (buildReport.summary.result == BuildResult.Succeeded)
                {
                    UnityEngine.Debug.LogFormat(kLogType + "BuildPlayer is finished successful : {0}", outputFullPath);

                    if (_openBuildPathAfterBuild)
                    {
                        ProjectBuilderUtil.RevealOutputInFinder(outputFullPath);
                    }
                }
                else
                {
                    UnityEngine.Debug.LogErrorFormat(kLogType + "BuildPlayer is failed : {0}", buildReport);
                    return false;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Build method for CUI(-executeMethod option).
        /// </summary>
        protected static void Build()
        {
            var arguments = ProjectBuilderUtil.GetBuilderFromExecuteArgument();
            ProjectBuilderUtil.StartBuild(arguments, false, false);
        }

#if UNITY_CLOUD_BUILD

        /// <summary>
        /// Pre-export method for Unity Cloud Build.
        /// </summary>
        static void PreExport(UnityEngine.CloudBuild.BuildManifestObject manifest)
        {
            Util.executeArguments[Util.OPT_DEV_BUILD_NUM] = manifest.GetValue("buildNumber", "unknown");
            Util.GetBuilderFromExecuteArgument().ApplySettings();
        }

#endif // UNITY_CLOUD_BUILD


        [System.Serializable]
        public class SceneSetting
        {
            public bool enable = true;
            public string name;
        }

        [System.Serializable]
        public enum BundleOptions
        {
            LZMA = BuildAssetBundleOptions.None,
            LZ4 = BuildAssetBundleOptions.ChunkBasedCompression,
            Uncompressed = BuildAssetBundleOptions.UncompressedAssetBundle,
        }
    }
}
