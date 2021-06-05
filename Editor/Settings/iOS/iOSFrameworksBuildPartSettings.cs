using System;
using UnityEditor;
using UnityEngine;
using UnityEditor.Callbacks;

#if UNITY_IOS
    using UnityEditor.iOS.Xcode;
#endif // UNITY_IOS

namespace Mobcast.Coffee.Build.Editor
{
    [System.Serializable]
    public class iOSFrameworksBuildPartSettings : IiOSFrameworksBuildPartSettings
    {
        private static readonly string[] s_AvailableFrameworks =
        {
            "AVFoundation.framework", // (com.apple.avfoundation)
            "AudioToolbox.framework", //  (com.apple.audio.toolbox.AudioToolbox)
            "CoreFoundation.framework", //  (com.apple.CoreFoundation)
            "CoreTelephony.framework", //  (com.apple.coretelephony)
            "CoreText.framework", //  (com.apple.CoreText)
            "Foundation.framework", //  (com.apple.Foundation)
            "IOKit.framework", // 
            "ImageIO.framework", //  (com.apple.ImageIO.framework)
            "MapKit.framework", //  (com.apple.MapKit)
            "MediaPlayer.framework", //  (com.apple.MediaPlayer)
            "QuartzCore.framework", //  (com.apple.QuartzCore)
            "UIKit.framework", //  (com.apple.UIKit)
            "iAd.framework", //  (com.apple.iAd)
        };
        
        
        /// <summary>Additional frameworks. If you have multiple definitions, separate with a semicolon(;)</summary>
        [Tooltip("Additional frameworks.\nIf you have multiple definitions, separate with a semicolon(;)")]
        [SerializeField] protected  string _frameworks = "";
        

        public string Frameworks => _frameworks;


        public virtual void ReadSettings(IiOSBuildSettings buildSettings)
        {
        }

        public virtual void ApplySettings(IiOSBuildSettings buildSettings)
        {
        }
        
        public virtual void DrawSetting(SerializedProperty settings, IiOSBuildSettings buildSettings)
        {
            EditorGUIExtensions.TextFieldWithTemplate(settings.FindPropertyRelative(nameof(_frameworks)),
                s_AvailableFrameworks, true);
        }
        
#if UNITY_IOS

        /// <summary>
        /// Raises the postprocess build event.
        /// </summary>
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

            var settings = current.FrameworksSettings;

            var frameworks = settings.Frameworks;

            // Add frameworks.
            if (!string.IsNullOrEmpty(frameworks))
            {
                foreach (string fw in frameworks.Split(';'))
                {
                    proj.AddFrameworkToProject(frameworkTargetGuid, fw, false);
                }
            }

            // Save XCode project.
            proj.WriteToFile(projPath);
        }
            
#endif // UNITY_IOS
    }
}
