using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEditor.Callbacks;

#if UNITY_IOS
    using UnityEditor.iOS.Xcode;
#endif // UNITY_IOS

namespace Mobcast.Coffee.Build.Editor
{
    [System.Serializable]
    public class iOSExportOptionsBuildPartSettings : IiOSExportOptionsBuildPartSettings
    {
        static readonly string[] s_AvailableExportMethods =
        {
            "app-store",
            "ad-hoc",
            "package",
            "enterprise",
            "development",
            "developer-id",
        };
        
        
        /// <summary>Generate exportOptions.plist automatically for xcodebuild (XCode7 and later).</summary>
        [Tooltip("Generate exportOptions.plist under build path for xcodebuild (XCode7 and later).")]
        [SerializeField] protected  bool _generateExportOptionPlist = false;

        /// <summary>The method of distribution, which can be set as any of the following: app-store, ad-hoc, package, enterprise, development, developer-id.</summary>
        [Tooltip(
            "The method of distribution, which can be set as any of the following:\napp-store, ad-hoc, package, enterprise, development, developer-id.")]
        [SerializeField] protected  string _exportMethod = "development";

        /// <summary>Option to include Bitcode.</summary>
        [Tooltip("Option to include Bitcode.")]
        [SerializeField] protected  bool _uploadBitcode = false;

        /// <summary>Option to include symbols in the generated ipa file.</summary>
        [Tooltip("Option to include symbols in the generated ipa file.")]
        [SerializeField] protected  bool _uploadSymbols = false;

        /// <summary>Entitlements file(*.entitlement).</summary>
        [Tooltip("Entitlements file(*.entitlements).")]
        [SerializeField] protected  string _entitlementsFile = "";
        

        public bool GenerateExportOptionPlist => _generateExportOptionPlist;

        public string ExportMethod => _exportMethod;

        public bool UploadBitcode => _uploadBitcode;

        public bool UploadSymbols => _uploadSymbols;

        public string EntitlementsFile => _entitlementsFile;

        

        public virtual void ReadSettings(IiOSBuildSettings buildSettings)
        {
        }

        public virtual void ApplySettings(IiOSBuildSettings buildSettings)
        {
        }
        
        public virtual void DrawSetting(SerializedProperty settings, IiOSBuildSettings buildSettings)
        {
            using (new EditorGUIExtensions.GroupScope("iOS Settings"))
            {
                // exportOptions.plist.
                EditorGUILayout.LabelField("exportOptions.plist Setting", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                {
                    var spGenerate = settings.FindPropertyRelative(nameof(_generateExportOptionPlist));
                    EditorGUILayout.PropertyField(spGenerate, new GUIContent("Generate Automatically"));
                    if (spGenerate.boolValue)
                    {
                        EditorGUIExtensions.TextFieldWithTemplate(settings.FindPropertyRelative(nameof(_exportMethod)),
                            s_AvailableExportMethods, false);
                        EditorGUILayout.PropertyField(settings.FindPropertyRelative(nameof(_uploadBitcode)));
                        EditorGUILayout.PropertyField(settings.FindPropertyRelative(nameof(_uploadSymbols)));
                    }
                }
                EditorGUI.indentLevel--;
            }
        }
        
#if UNITY_IOS

        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
        {
            var current = ProjectBuilderUtil.currentBuilder.GetiOSSettings;
            if (buildTarget != BuildTarget.iOS || current == null)
            {
                return;
            }

            var exportOptionsSettings = current.ExportOptionsSettings;
            
            var signingSettings = current.SigningSettings;
            string developerTeamId = signingSettings.DeveloperTeamId;

            // Generate exportOptions.plist automatically.
            if (exportOptionsSettings.GenerateExportOptionPlist)
            {
                var plist = new PlistDocument();
                plist.root.SetString("teamID", developerTeamId);
                plist.root.SetString("method", exportOptionsSettings.ExportMethod);
                plist.root.SetBoolean("uploadBitcode", exportOptionsSettings.UploadBitcode);
                plist.root.SetBoolean("uploadSymbols", exportOptionsSettings.UploadSymbols);

                // Generate exportOptions.plist into build path.
                plist.WriteToFile(Path.Combine(path, "exportOptions.plist"));
            }

            // Modify XCode project.
            string projPath = PBXProject.GetPBXProjectPath(path);
            PBXProject proj = new PBXProject();
            proj.ReadFromFile(projPath);
            
#if UNITY_2019_3_OR_NEWER
            string targetGuid = proj.GetUnityMainTargetGuid();
            string frameworkTargetGuid = proj.GetUnityFrameworkTargetGuid();
#else
            string targetGuid = proj.TargetGuidByName(PBXProject.GetUnityTargetName());
            string frameworkTargetGuid = targetGuid;
#endif

            // Modify build properties.
            proj.SetBuildProperty(targetGuid, "ENABLE_BITCODE", exportOptionsSettings.UploadBitcode ? "YES" : "NO");

            // Save XCode project.
            proj.WriteToFile(projPath);
        }
        
#endif // UNITY_IOS
    }
}
