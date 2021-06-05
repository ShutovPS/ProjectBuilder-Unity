using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Mobcast.Coffee.Build.Editor
{
    [System.Serializable]
    public class AndroidTargetBuildPartSettings : IAndroidTargetBuildPartSettings
    {
        [SerializeField] protected EScriptingBackend _scriptingBackend = EScriptingBackend.Mono2x;

        [SerializeField] protected EAndroidArchitecture _targetArchitecture = EAndroidArchitecture.ARMv7;

        [SerializeField] protected EAndroidBuildMode _buildMode = EAndroidBuildMode.APK;


        public EScriptingBackend ScriptingBackend => _scriptingBackend;
        public EAndroidArchitecture TargetArchitecture => _targetArchitecture;

        public EAndroidBuildMode BuildMode => _buildMode;


        protected static readonly Dictionary<EScriptingBackend, EAndroidArchitecture> _availableArchitectures =
            new Dictionary<EScriptingBackend, EAndroidArchitecture>()
            {
                {EScriptingBackend.Mono2x, EAndroidArchitecture.ARMv7},
                {EScriptingBackend.IL2CPP, EAndroidArchitecture.All}
            };


        public virtual void ReadSettings(IAndroidBuildSettings buildSettings)
        {
            ReadBuildTargetSettings(buildSettings);
            ReadBuildModeSettings(buildSettings);
        }

        protected virtual void ReadBuildTargetSettings(IAndroidBuildSettings buildSettings)
        {
            var scriptingBackend = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android);
            _scriptingBackend = (EScriptingBackend)scriptingBackend;

            var targetArchitecture = PlayerSettings.Android.targetArchitectures;
            _targetArchitecture = (EAndroidArchitecture)targetArchitecture;
        }

        protected virtual void ReadBuildModeSettings(IAndroidBuildSettings buildSettings)
        {
            bool exportAsGoogleAndroidProject = EditorUserBuildSettings.exportAsGoogleAndroidProject;
            bool buildAppBundle = EditorUserBuildSettings.buildAppBundle;

            if (exportAsGoogleAndroidProject)
            {
                _buildMode = EAndroidBuildMode.ANDROID_PROJECT;
            }
            else if (buildAppBundle)
            {
                _buildMode = EAndroidBuildMode.GOOGLE_BUNDLE;
            }
            else
            {
                _buildMode = EAndroidBuildMode.APK;
            }
        }

        public virtual void ApplySettings(IAndroidBuildSettings buildSettings)
        {
            ApplyBuildTargetSettings(buildSettings);
            ApplyBuildModeSettings(buildSettings);
        }

        public virtual void ApplyBuildTargetSettings(IAndroidBuildSettings buildSettings)
        {
            var scriptingBackend = (ScriptingImplementation)_scriptingBackend;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, scriptingBackend);

            var targetArchitecture = (AndroidArchitecture)_targetArchitecture;
            PlayerSettings.Android.targetArchitectures = targetArchitecture;
        }

        public virtual void ApplyBuildModeSettings(IAndroidBuildSettings buildSettings)
        {
            switch (_buildMode)
            {
                case EAndroidBuildMode.APK:
                    EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
                    EditorUserBuildSettings.buildAppBundle = false;
                    break;
                case EAndroidBuildMode.ANDROID_PROJECT:
                    EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
                    EditorUserBuildSettings.buildAppBundle = false;
                    break;
                case EAndroidBuildMode.GOOGLE_BUNDLE:
                    EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
                    EditorUserBuildSettings.buildAppBundle = true;
                    break;
            }
        }


#region GUI

        /// <summary>
        /// Draws the ios settings.
        /// </summary>
        public virtual void DrawSetting(SerializedProperty settings, IAndroidBuildSettings buildSettings)
        {
            EditorGUILayout.LabelField("Target", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            {
                DrawTargetBackend(settings, buildSettings);

                DrawTargetArchitecture(settings, buildSettings);

                DrawTargetBuildMode(settings, buildSettings);
            }
            EditorGUI.indentLevel--;
        }

        protected virtual void DrawTargetBackend(SerializedProperty settings, IAndroidBuildSettings buildSettings)
        {
            var scriptingBackend = settings.FindPropertyRelative(nameof(_scriptingBackend));
            EditorGUILayout.PropertyField(scriptingBackend, new GUIContent("Scripting Backend"));
        }

        protected virtual void DrawTargetArchitecture(SerializedProperty settings, IAndroidBuildSettings buildSettings)
        {
            EditorGUILayout.LabelField("Target Architecture", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            {
                var targetArchitectureProperty = settings.FindPropertyRelative(nameof(_targetArchitecture));
                var targetArchitecture = (EAndroidArchitecture)targetArchitectureProperty.longValue;

                var scriptingBackendProperty = settings.FindPropertyRelative(nameof(_scriptingBackend));
                var scriptingBackend = (EScriptingBackend)scriptingBackendProperty.longValue;

                bool taChanged = false;

                var names = Enum.GetNames(typeof(EAndroidArchitecture));
                var values = (EAndroidArchitecture[])Enum.GetValues(typeof(EAndroidArchitecture));
                for (int i = 1; i < values.Length - 1; i++)
                {
                    var architecture = values[i];
                    bool isArchitectureEnable = targetArchitecture.HasFlag(architecture);

                    GUI.changed = false;

                    if (_availableArchitectures.TryGetValue(scriptingBackend, out var architectures))
                    {
                        bool isAvailableArchitecture = architectures.HasFlag(architecture);

                        EditorGUI.BeginDisabledGroup(!isAvailableArchitecture);

                        if (!isAvailableArchitecture)
                        {
                            if (isArchitectureEnable)
                            {
                                GUI.changed = true;
                                isArchitectureEnable = false;
                            }
                        }
                    }

                    isArchitectureEnable = EditorGUILayout.Toggle(new GUIContent(names[i]), isArchitectureEnable);

                    if (GUI.changed)
                    {
                        if (isArchitectureEnable)
                        {
                            targetArchitecture |= values[i];
                        }
                        else
                        {
                            targetArchitecture ^= values[i];
                        }

                        taChanged = true;
                    }

                    EditorGUI.EndDisabledGroup();
                }

                if (taChanged)
                {
                    targetArchitectureProperty.longValue = (long)targetArchitecture;
                }
            }
            EditorGUI.indentLevel--;
        }

        protected virtual void DrawTargetBuildMode(SerializedProperty settings, IAndroidBuildSettings buildSettings)
        {
            var buildMode = settings.FindPropertyRelative(nameof(_buildMode));
            EditorGUILayout.PropertyField(buildMode, new GUIContent("Build Mode"));
        }

#endregion GUI


    }
}
