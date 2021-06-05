using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.Callbacks;

#if UNITY_IOS
    using UnityEditor.iOS.Xcode;
#endif // UNITY_IOS

namespace Mobcast.Coffee.Build.Editor
{
    [System.Serializable]
    public class iOSLanguagesBuildPartSettings : IiOSLanguagesBuildPartSettings
    {
        static readonly string[] s_AvailableLanguages =
        {
            "ar",
            "bn",
            "de",
            "en",
            "es",
            "fr",
            "hi",
            "it",
            "ja",
            "pt",
            "ru",
            "zh",
        };
        
        
        /// <summary>Support languages. If you have multiple definitions, separate with a semicolon(;)</summary>
        [Tooltip("Support languages.\nIf you have multiple definitions, separate with a semicolon(;)")]
        [SerializeField] protected  string _languages = "en";

        
        public string Languages => _languages;


        public virtual void ReadSettings(IiOSBuildSettings buildSettings)
        {
        }

        public virtual void ApplySettings(IiOSBuildSettings buildSettings)
        {
        }
        
        public virtual void DrawSetting(SerializedProperty settings, IiOSBuildSettings buildSettings)
        {
            EditorGUIExtensions.TextFieldWithTemplate(settings.FindPropertyRelative(nameof(_languages)), s_AvailableLanguages,
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

            var languagesSettings = current.LanguagesSettings;

            // Support languages.
            string[] languages = languagesSettings.Languages.Split(';');
            if (languages.Length > 0)
            {
                // Load Info.plist
                string infoPlistPath = Path.Combine(path, "Info.plist");
                var plist = new PlistDocument();
                plist.ReadFromFile(infoPlistPath);

                // Set default language.
                plist.root.SetString("CFBundleDevelopmentRegion", languages[0]);

                var bundleLocalizations =
                    plist.root.values.ContainsKey("CFBundleLocalizations")
                        ? plist.root.values["CFBundleLocalizations"].AsArray()
                        : plist.root.CreateArray("CFBundleLocalizations");

                // Add support language.
                foreach (string lang in languages)
                {
                    if (bundleLocalizations.values.All(x => x.AsString() != lang))
                    {
                        bundleLocalizations.AddString(lang);
                    }
                }

                // Save Info.plist
                plist.WriteToFile(infoPlistPath);
            }
        }
            
#endif // UNITY_IOS
    }
}
