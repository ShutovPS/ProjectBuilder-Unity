#if UNITY_2019_3_OR_NEWER || UNITY_2020_1_OR_NEWER
    #define SYMBOLS_ZIP_AVAILABLE
#endif // UNITY_2019_3_OR_NEWER || UNITY_2020_1_OR_NEWER

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Mobcast.Coffee.Build.Editor
{
    [System.Serializable]
    public class AndroidBuildSettings : IAndroidBuildSettings
    {
        [SerializeField]
        protected AndroidKeystoreBuildPartSettings _keystoreSettings = new AndroidKeystoreBuildPartSettings();
        [SerializeField]
        protected AndroidTargetBuildPartSettings _targetSettings = new AndroidTargetBuildPartSettings();

        [SerializeField]
        protected AndroidSymbolsZipBuildPartSettings _symbolsZipSettings = new AndroidSymbolsZipBuildPartSettings();


        public BuildTarget GetBuildTarget
        {
            get { return BuildTarget.Android; }
        }

        public Texture GetIcon
        {
            get { return EditorGUIUtility.FindTexture("BuildSettings.Android.Small"); }
        }

        public AndroidKeystoreBuildPartSettings KeystoreSettings => _keystoreSettings;

        public AndroidTargetBuildPartSettings TargetSettings => _targetSettings;

        public AndroidSymbolsZipBuildPartSettings SymbolsZipSettings => _symbolsZipSettings;


        public virtual void ReadSettings(IProjectBuilder builder)
        {
            _keystoreSettings.ReadSettings(this);
            _targetSettings.ReadSettings(this);
            _symbolsZipSettings.ReadSettings(this);
        }

        public virtual void ApplySettings(IProjectBuilder builder)
        {
            _keystoreSettings.ApplySettings(this);
            _targetSettings.ApplySettings(this);
            _symbolsZipSettings.ApplySettings(this);

            ApplyBuildVersionSettings(builder);
        }

        public virtual void ApplyBuildVersionSettings(IProjectBuilder builder)
        {
            string versionCodeLong = BuildPathUtils.GetVersionCodeLong(builder);
            PlayerSettings.Android.bundleVersionCode = int.Parse(versionCodeLong);
        }


#region GUI

        /// <summary>
        /// Draws the android settings.
        /// </summary>
        public virtual void DrawSetting(SerializedProperty settings)
        {
            using (new EditorGUIExtensions.GroupScope("Android Settings"))
            {
                DrawTarget(settings);

                DrawKeystore(settings);

                DrawSymbolsZip(settings);
            }
        }

        protected virtual void DrawTarget(SerializedProperty settings)
        {
            var targetSettings = settings.FindPropertyRelative(nameof(_targetSettings));

            _targetSettings.DrawSetting(targetSettings, this);
        }

        protected virtual void DrawKeystore(SerializedProperty settings)
        {
            var keystoreSettings = settings.FindPropertyRelative(nameof(_keystoreSettings));

            _keystoreSettings.DrawSetting(keystoreSettings, this);
        }

        protected virtual void DrawSymbolsZip(SerializedProperty settings)
        {
            var symbolsZipSettings = settings.FindPropertyRelative(nameof(_symbolsZipSettings));

            _symbolsZipSettings.DrawSetting(symbolsZipSettings, this);
        }

#endregion GUI


    }
}
