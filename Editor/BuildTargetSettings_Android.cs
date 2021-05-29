using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Mobcast.Coffee.Build
{
    [Serializable]
    public class BuildTargetSettings_Android : IBuildTargetSettings
    {
        public BuildTarget buildTarget
        {
            get { return BuildTarget.Android; }
        }

        public Texture icon
        {
            get { return EditorGUIUtility.FindTexture("BuildSettings.Android.Small"); }
        }


#region KEYSTORE

        /// <summary>Enable application signing with a custom keystore.</summary>
        [Tooltip("Enable application signing with a custom keystore.")]
        [SerializeField] private bool useCustomKeystore = false;

        /// <summary>Keystore file path.</summary>
        [Tooltip("Keystore file path.")]
        [SerializeField] private string keystoreFile = "";

        /// <summary>Keystore password.</summary>
        [Tooltip("Keystore password.")]
        [SerializeField] private string keystorePassword = "";

        /// <summary>Keystore alias name.</summary>
        [Tooltip("Keystore alias name.")]
        [SerializeField] private string keystoreAliasName = "";

        /// <summary>Keystore alias password.</summary>
        [Tooltip("Keystore alias password.")]
        [SerializeField] private string keystoreAliasPassword = "";

#endregion KEYSTORE


#region TARGET

        [SerializeField] private EScriptingBackend _scriptingBackend = EScriptingBackend.Mono2x;

        [SerializeField] private AndroidArchitecture _targetArchitecture = AndroidArchitecture.ARMv7;

        [SerializeField] private EBuildMode _buildMode = EBuildMode.APK;

#endregion TARGET


        [NonSerialized] private static bool _hidePasswords = true;

        public void Reset()
        {
            useCustomKeystore = PlayerSettings.Android.useCustomKeystore;

            keystoreFile = PlayerSettings.Android.keystoreName.Replace("\\", "/")
                .Replace(Util.projectDir + "/", "");
            keystorePassword = PlayerSettings.Android.keystorePass;
            keystoreAliasName = PlayerSettings.Android.keyaliasName;
            keystoreAliasPassword = PlayerSettings.Android.keyaliasPass;


            _scriptingBackend =
                (EScriptingBackend)PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android);
            _targetArchitecture = (AndroidArchitecture)PlayerSettings.Android.targetArchitectures;


            bool exportAsGoogleAndroidProject = EditorUserBuildSettings.exportAsGoogleAndroidProject;
            bool buildAppBundle = EditorUserBuildSettings.buildAppBundle;

            if (exportAsGoogleAndroidProject)
            {
                _buildMode = EBuildMode.ANDROID_PROJECT;
            }
            else if (buildAppBundle)
            {
                _buildMode = EBuildMode.GOOGLE_BUNDLE;
            }
            else
            {
                _buildMode = EBuildMode.APK;
            }
        }

        public void ApplySettings(ProjectBuilder builder)
        {
            PlayerSettings.Android.bundleVersionCode = int.Parse(builder.FullVersionCode);

            PlayerSettings.Android.useCustomKeystore = useCustomKeystore;

            PlayerSettings.Android.keystoreName = keystoreFile;
            PlayerSettings.Android.keystorePass = keystorePassword;
            PlayerSettings.Android.keyaliasName = keystoreAliasName;
            PlayerSettings.Android.keyaliasPass = keystoreAliasPassword;

            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android,
                (ScriptingImplementation)_scriptingBackend);
            PlayerSettings.Android.targetArchitectures = (AndroidArchitecture)_targetArchitecture;

            switch (_buildMode)
            {
                case EBuildMode.APK:
                    EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
                    EditorUserBuildSettings.buildAppBundle = false;
                    break;
                case EBuildMode.ANDROID_PROJECT:
                    EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
                    EditorUserBuildSettings.buildAppBundle = false;
                    break;
                case EBuildMode.GOOGLE_BUNDLE:
                    EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
                    EditorUserBuildSettings.buildAppBundle = true;
                    break;
            }
        }

        /// <summary>
        /// Draws the ios settings.
        /// </summary>
        public void DrawSetting(SerializedObject serializedObject)
        {
            var settings = serializedObject.FindProperty("androidSettings");

            using (new EditorGUIEx.GroupScope("Android Settings"))
            {
                DrawTarget(settings);

                DrawKeystore(settings);
            }
        }

        private static void DrawTarget(SerializedProperty settings)
        {
            EditorGUILayout.LabelField("Target", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            {
                var scriptingBackend = settings.FindPropertyRelative("_scriptingBackend");
                EditorGUILayout.PropertyField(scriptingBackend, new GUIContent("Scripting Backend"));
                var sb = (EScriptingBackend)scriptingBackend.longValue;

                EditorGUILayout.LabelField("Target Architecture", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                {
                    var targetArchitecture = settings.FindPropertyRelative("_targetArchitecture");
                    var ta = (AndroidArchitecture)targetArchitecture.longValue;
                    bool taChanged = false;
                    // ta = (AndroidArchitecture) EditorGUILayout.EnumFlagsField(
                    // 	new GUIContent("Target Architecture"), ta);

                    var names = Enum.GetNames(typeof(AndroidArchitecture));
                    var values = (AndroidArchitecture[])Enum.GetValues(typeof(AndroidArchitecture));
                    for (int i = 1; i < values.Length - 1; i++)
                    {
                        var value = values[i];
                        bool boolValue = ta.HasFlag(values[i]);

                        GUI.changed = false;

                        if (_available.TryGetValue(value, out var backend))
                        {
                            if (backend != sb)
                            {
                                GUI.enabled = false;

                                if (boolValue)
                                {
                                    GUI.changed = true;
                                    boolValue = false;
                                }
                            }
                        }

                        boolValue = EditorGUILayout.Toggle(new GUIContent(names[i]), boolValue);

                        if (GUI.changed)
                        {
                            if (boolValue)
                            {
                                ta |= values[i];
                            }
                            else
                            {
                                ta ^= values[i];
                            }

                            taChanged = true;
                        }

                        GUI.enabled = true;
                    }

                    if (taChanged)
                    {
                        targetArchitecture.longValue = (long)ta;
                    }
                }
                EditorGUI.indentLevel--;

                var buildMode = settings.FindPropertyRelative("_buildMode");
                EditorGUILayout.PropertyField(buildMode, new GUIContent("Build Mode"));
            }
            EditorGUI.indentLevel--;
        }

        private static void DrawKeystore(SerializedProperty settings)
        {
            EditorGUILayout.LabelField("Keystore", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            {
                var useCustomKeystore = settings.FindPropertyRelative("useCustomKeystore");
                EditorGUILayout.PropertyField(useCustomKeystore, new GUIContent("Use custom keystore"));

                EditorGUI.BeginDisabledGroup(!useCustomKeystore.boolValue);

                var keystoreFile = settings.FindPropertyRelative("keystoreFile");
                EditorGUIEx.FilePathField(keystoreFile, "Select keystore file.", "", "");

                DrawPassword(settings, "keystorePassword", new GUIContent("Keystore Password"));

                var keystoreAliasName = settings.FindPropertyRelative("keystoreAliasName");
                EditorGUILayout.PropertyField(keystoreAliasName, new GUIContent("Alias"));

                DrawPassword(settings, "keystoreAliasPassword", new GUIContent("Alias Password"));

                EditorGUI.EndDisabledGroup();

                _hidePasswords = EditorGUILayout.Toggle(new GUIContent("Hide Passwords"), _hidePasswords);
            }
            EditorGUI.indentLevel--;
        }

        private static void DrawPassword(SerializedProperty settings, string propertyId, GUIContent label = null)
        {
            var property = settings.FindPropertyRelative(propertyId);

            if (label == null)
            {
                label = new GUIContent(propertyId);
            }

            if (_hidePasswords)
            {
                property.stringValue = EditorGUILayout.PasswordField(label, property.stringValue);
            }
            else
            {

                EditorGUILayout.PropertyField(property, label);
            }
        }

        private static readonly Dictionary<AndroidArchitecture, EScriptingBackend> _available =
            new Dictionary<AndroidArchitecture, EScriptingBackend>()
            {
                {AndroidArchitecture.ARM64, EScriptingBackend.IL2CPP}
            };

        [Serializable]
        private enum EScriptingBackend
        {
            Mono2x = ScriptingImplementation.Mono2x,
            IL2CPP = ScriptingImplementation.IL2CPP,
        }

        [Serializable]
        private enum EBuildMode
        {
            APK,
            ANDROID_PROJECT,
            GOOGLE_BUNDLE,
        }
    }
}
