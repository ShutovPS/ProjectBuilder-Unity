using System;
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
    public class iOSServicesBuildPartSettings : IiOSServicesBuildPartSettings
    {
        static readonly string[] s_AvailableServices =
        {
            "com.apple.ApplePay",
            "com.apple.ApplicationGroups.iOS",
            "com.apple.BackgroundModes",
            "com.apple.DataProtection",
            "com.apple.GameCenter",
            "com.apple.GameControllers.appletvos",
            "com.apple.HealthKit",
            "com.apple.HomeKit",
            "com.apple.InAppPurchase",
            "com.apple.InterAppAudio",
            "com.apple.Keychain",
            "com.apple.Maps.iOS",
            "com.apple.NetworkExtensions",
            "com.apple.Push",
            "com.apple.SafariKeychain",
            "com.apple.Siri",
            "com.apple.VPNLite",
            "com.apple.WAC",
            "com.apple.Wallet",
            "com.apple.iCloud",
        };
        
        
        /// <summary>Apple services. If you have multiple definitions, separate with a semicolon(;)</summary>
        [Tooltip("Apple services.\nIf you have multiple definitions, separate with a semicolon(;)")]
        [SerializeField] protected  string _services = "";
        

        public string Services => _services;
        


        public virtual void ReadSettings(IiOSBuildSettings buildSettings)
        {
        }

        public virtual void ApplySettings(IiOSBuildSettings buildSettings)
        {
        }
        
        public virtual void DrawSetting(SerializedProperty settings, IiOSBuildSettings buildSettings)
        {
            EditorGUIExtensions.TextFieldWithTemplate(settings.FindPropertyRelative(nameof(_services)), s_AvailableServices,
                true);
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

            var servicesSettings = current.ServicesSettings;
            
            var signingSettings = current.SigningSettings;
            string developerTeamId = signingSettings.DeveloperTeamId;

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

            string services = servicesSettings.Services;

            // Activate services.
            if (!string.IsNullOrEmpty(services) && !string.IsNullOrEmpty(developerTeamId))
            {
                var reg = new Regex("(\\t*SystemCapabilities = {\\n)((.*{\\n.*\\n.*};\\n)+)");
                string replaceText =
                    string.Format("\nDevelopmentTeam = {0};\n$0{1}\n"
                        , developerTeamId
                        , services.Split(';').Select(x => x + " = {enabled = 1;};").Aggregate((a, b) => a + b)
                    );
                proj.ReadFromString(reg.Replace(proj.WriteToString(), replaceText));
            }

            // Save XCode project.
            proj.WriteToFile(projPath);
        }
        
#endif // UNITY_IOS
    }
}
