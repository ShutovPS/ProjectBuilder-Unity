using System;
using System.IO;
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
    public class iOSSigningBuildPartSettings : IiOSSigningBuildPartSettings
    {
        /// <summary>Enable automatically sign.</summary>
        [Tooltip("Enable automatically sign.")]
        [SerializeField] protected bool _automaticallySign = false;

        /// <summary>Developer Team Id.</summary>
        [Tooltip("Developer Team Id.")]
        [SerializeField] protected  string _developerTeamId = "";

        /// <summary>Code Sign Identifier.</summary>
        [Tooltip("Code Sign Identifier.")]
        [SerializeField] protected  string _codeSignIdentity = "";
        
        /// <summary>Provisioning Profile Id. For example: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx</summary>
        [Tooltip("Provisioning Profile Id.\nFor example: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx")]
        [SerializeField] protected  string _profileId = "";
        
        /// <summary>Provisioning Profile Specifier. For example: com campany app_name</summary>
        [Tooltip("Provisioning Profile Specifier.\nFor example: com campany app_name")]
        [SerializeField] protected  string _profileSpecifier = "";
        

        public bool AutomaticallySign => _automaticallySign;

        public string DeveloperTeamId => _developerTeamId;

        public string CodeSignIdentity => _codeSignIdentity;

        public string ProfileId => _profileId;

        public string ProfileSpecifier => _profileSpecifier;
        


        public void ReadSettings(IiOSBuildSettings buildSettings)
        {
#if UNITY_5_4_OR_NEWER
            _developerTeamId = PlayerSettings.iOS.appleDeveloperTeamID;
#endif // UNITY_5_4_OR_NEWER
            
#if UNITY_5_5_OR_NEWER
            _automaticallySign = PlayerSettings.iOS.appleEnableAutomaticSigning;
            _profileId = PlayerSettings.iOS.iOSManualProvisioningProfileID;
#endif // UNITY_5_5_OR_NEWER
        }

        public virtual void ApplySettings(IiOSBuildSettings buildSettings)
        {
#if UNITY_5_4_OR_NEWER
            PlayerSettings.iOS.appleDeveloperTeamID = _developerTeamId;
#endif // UNITY_5_4_OR_NEWER
#if UNITY_5_5_OR_NEWER
            PlayerSettings.iOS.appleEnableAutomaticSigning = _automaticallySign;
            if (!_automaticallySign)
            {
                PlayerSettings.iOS.iOSManualProvisioningProfileID = _profileId;
            }
#endif // UNITY_5_5_OR_NEWER
        }
        
        public virtual void DrawSetting(SerializedProperty settings, IiOSBuildSettings buildSettings)
        {
            EditorGUILayout.LabelField("Signing", EditorStyles.boldLabel);
            var spAutomaticallySign = settings.FindPropertyRelative(nameof(_automaticallySign));
            EditorGUI.indentLevel++;
            {
                EditorGUILayout.PropertyField(spAutomaticallySign);
                EditorGUILayout.PropertyField(settings.FindPropertyRelative(nameof(_developerTeamId)));
                if (!spAutomaticallySign.boolValue)
                {
                    EditorGUILayout.PropertyField(settings.FindPropertyRelative(nameof(_codeSignIdentity)));
                    EditorGUILayout.PropertyField(settings.FindPropertyRelative(nameof(_profileId)));
                    EditorGUILayout.PropertyField(settings.FindPropertyRelative(nameof(_profileSpecifier)));
                }
            }
            EditorGUI.indentLevel--;
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

            var signingSettings = current.SigningSettings;

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

            string developerTeamId = signingSettings.DeveloperTeamId;
            bool automaticallySign = signingSettings.AutomaticallySign;
            string profileId = signingSettings.ProfileId;
            string profileSpecifier = signingSettings.ProfileSpecifier;
            string codeSignIdentity = signingSettings.CodeSignIdentity;

            // Modify build properties.
            if (!string.IsNullOrEmpty(developerTeamId))
            {
                proj.SetBuildProperty(targetGuid, "DEVELOPMENT_TEAM", developerTeamId);
            }

            if (!automaticallySign && !string.IsNullOrEmpty(profileId))
            {
                proj.SetBuildProperty(targetGuid, "PROVISIONING_PROFILE", profileId);
            }
            if (!automaticallySign && !string.IsNullOrEmpty(profileSpecifier))
            {
                proj.SetBuildProperty(targetGuid, "PROVISIONING_PROFILE_SPECIFIER", profileSpecifier);
            }
            if (!automaticallySign && !string.IsNullOrEmpty(codeSignIdentity))
            {
                proj.SetBuildProperty(targetGuid, "CODE_SIGN_IDENTITY", codeSignIdentity);
            }

            // Save XCode project.
            proj.WriteToFile(projPath);
        }
        
#endif // UNITY_IOS
    }
}
