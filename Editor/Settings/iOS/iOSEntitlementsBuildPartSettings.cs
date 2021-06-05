using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.Callbacks;

#if UNITY_IOS
    using UnityEditor.iOS.Xcode;
#endif // UNITY_IOS

namespace Mobcast.Coffee.Build.Editor
{
    [System.Serializable]
    public class iOSEntitlementsBuildPartSettings : IiOSEntitlementsBuildPartSettings
    {
        /// <summary>Entitlements file(*.entitlement).</summary>
        [Tooltip("Entitlements file(*.entitlements).")]
        [SerializeField] protected  string _entitlementsFile = "";
        

        public string EntitlementsFile => _entitlementsFile;
        

        public virtual void ReadSettings(IiOSBuildSettings buildSettings)
        {
        }

        public virtual void ApplySettings(IiOSBuildSettings buildSettings)
        {
        }
        
        public virtual void DrawSetting(SerializedProperty settings, IiOSBuildSettings buildSettings)
        {
            EditorGUIExtensions.FilePathField(settings.FindPropertyRelative(nameof(_entitlementsFile)),
                "Select entitlement file.", "", "entitlements");
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

            var entitlementsSettings = current.EntitlementsSettings;

            string entitlementsFile = entitlementsSettings.EntitlementsFile;

            // Set entitlement file.
            if (!string.IsNullOrEmpty(entitlementsFile))
            {
                string filename = Path.GetFileName(entitlementsFile);
                if (!proj.ContainsFileByProjectPath(filename))
                {
                    proj.AddFile("../" + entitlementsFile, filename);
                }
                proj.SetBuildProperty(targetGuid, "CODE_SIGN_ENTITLEMENTS", filename);
            }

            // Save XCode project.
            proj.WriteToFile(projPath);
        }
        
#endif // UNITY_IOS
    }
}
