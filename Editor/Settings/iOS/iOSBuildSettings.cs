using System;
using UnityEditor;
using UnityEngine;
using UnityEditor.Callbacks;

namespace Mobcast.Coffee.Build.Editor
{
    [System.Serializable]
    public class iOSBuildSettings : IiOSBuildSettings
    {
        [SerializeField] protected iOSLanguagesBuildPartSettings _languagesSettings = new iOSLanguagesBuildPartSettings();
        [SerializeField] protected iOSFrameworksBuildPartSettings _frameworksSettings = new iOSFrameworksBuildPartSettings();
        [SerializeField] protected iOSServicesBuildPartSettings _servicesSettings = new iOSServicesBuildPartSettings();
        [SerializeField] protected iOSEntitlementsBuildPartSettings _entitlementsSettings = new iOSEntitlementsBuildPartSettings();
        [SerializeField] protected iOSSigningBuildPartSettings _signingSettings = new iOSSigningBuildPartSettings();
        [SerializeField] protected iOSExportOptionsBuildPartSettings _exportOptionsSettings = new iOSExportOptionsBuildPartSettings();
        
        
        public IiOSLanguagesBuildPartSettings LanguagesSettings => _languagesSettings;
        public IiOSFrameworksBuildPartSettings FrameworksSettings => _frameworksSettings;
        public IiOSServicesBuildPartSettings ServicesSettings => _servicesSettings;
        public IiOSEntitlementsBuildPartSettings EntitlementsSettings => _entitlementsSettings;
        public IiOSSigningBuildPartSettings SigningSettings => _signingSettings;
        public IiOSExportOptionsBuildPartSettings ExportOptionsSettings => _exportOptionsSettings;

        
        public BuildTarget GetBuildTarget
        {
            get { return BuildTarget.iOS; }
        }

        public Texture GetIcon
        {
            get { return EditorGUIUtility.FindTexture("BuildSettings.iPhone.Small"); }
        }


        public virtual void ReadSettings(IProjectBuilder builder)
        {
            _languagesSettings.ReadSettings(this);
            _frameworksSettings.ReadSettings(this);
            _servicesSettings.ReadSettings(this);
            _entitlementsSettings.ReadSettings(this);
            _signingSettings.ReadSettings(this);
            _exportOptionsSettings.ReadSettings(this);
        }

        public virtual void ApplySettings(IProjectBuilder builder)
        {
            _languagesSettings.ApplySettings(this);
            _frameworksSettings.ApplySettings(this);
            _servicesSettings.ApplySettings(this);
            _entitlementsSettings.ApplySettings(this);
            _signingSettings.ApplySettings(this);
            _exportOptionsSettings.ApplySettings(this);

            ApplyBuildVersionSettings(builder);
        }

        public virtual void ApplyBuildVersionSettings(IProjectBuilder builder)
        {
            string versionCodeLong = BuildPathUtils.GetVersionCodeLong(builder);
            PlayerSettings.iOS.buildNumber = versionCodeLong;
        }
        
        public virtual void DrawSetting(SerializedProperty settings)
        {
            using (new EditorGUIExtensions.GroupScope("iOS Settings"))
            {
                // XCode Project.
                EditorGUILayout.LabelField("XCode Project", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                {
                    DrawLanguagesSettings(settings);
                    DrawFrameworksSettings(settings);
                    DrawServicesSettings(settings);
                    DrawEntitlementsSettings(settings);
                }
                EditorGUI.indentLevel--;

                DrawSigningSettings(settings);

                DrawExportOptionsSettings(settings);
            }
        }

        protected virtual void DrawLanguagesSettings(SerializedProperty settings)
        {
            var languagesSettings = settings.FindPropertyRelative(nameof(_languagesSettings));

            _languagesSettings.DrawSetting(languagesSettings, this);
        }

        protected virtual void DrawFrameworksSettings(SerializedProperty settings)
        {
            var frameworksSettings = settings.FindPropertyRelative(nameof(_frameworksSettings));

            _frameworksSettings.DrawSetting(frameworksSettings, this);
        }

        protected virtual void DrawServicesSettings(SerializedProperty settings)
        {
            var servicesSettings = settings.FindPropertyRelative(nameof(_servicesSettings));

            _servicesSettings.DrawSetting(servicesSettings, this);
        }

        protected virtual void DrawEntitlementsSettings(SerializedProperty settings)
        {
            var entitlementsSettings = settings.FindPropertyRelative(nameof(_entitlementsSettings));

            _entitlementsSettings.DrawSetting(entitlementsSettings, this);
        }

        protected virtual void DrawSigningSettings(SerializedProperty settings)
        {
            var signingSettings = settings.FindPropertyRelative(nameof(_signingSettings));

            _signingSettings.DrawSetting(signingSettings, this);
        }

        protected virtual void DrawExportOptionsSettings(SerializedProperty settings)
        {
            var exportOptionsSettings = settings.FindPropertyRelative(nameof(_exportOptionsSettings));

            _exportOptionsSettings.DrawSetting(exportOptionsSettings, this);
        }
        
#if UNITY_IOS

        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
        {
        }
        
#endif // UNITY_IOS
    }
}
