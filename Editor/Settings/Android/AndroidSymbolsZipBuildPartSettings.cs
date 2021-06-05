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
    public class AndroidSymbolsZipBuildPartSettings : IAndroidSymbolsZipBuildPartSettings
    {
        /// <summary>Set to true to create a symbols.zip file in the same location as the .apk or .aab file.</summary>
        [Tooltip("Set to true to create a symbols.zip file in the same location as the .apk or .aab file.")]
        [SerializeField] protected bool _createSymbolsZip = false;


        public virtual void ReadSettings(IAndroidBuildSettings buildSettings)
        {
#if SYMBOLS_ZIP_AVAILABLE
            _createSymbolsZip = EditorUserBuildSettings.androidCreateSymbolsZip;
#endif // SYMBOLS_ZIP_AVAILABLE
        }

        public virtual void ApplySettings(IAndroidBuildSettings buildSettings)
        {
#if SYMBOLS_ZIP_AVAILABLE
            EditorUserBuildSettings.androidCreateSymbolsZip = _createSymbolsZip;
#endif // SYMBOLS_ZIP_AVAILABLE
        }


#region GUI

        public virtual void DrawSetting(SerializedProperty settings, IAndroidBuildSettings buildSettings)
        {
#if SYMBOLS_ZIP_AVAILABLE

            EditorGUILayout.LabelField("symbols.zip", EditorStyles.boldLabel);

            var targetSettings = buildSettings.TargetSettings;
            var scriptingBackend = targetSettings.ScriptingBackend;

            EditorGUI.BeginDisabledGroup(scriptingBackend != EScriptingBackend.IL2CPP);

            EditorGUI.indentLevel++;
            {
                var createSymbolsZip = settings.FindPropertyRelative("_createSymbolsZip");
                EditorGUILayout.PropertyField(createSymbolsZip, new GUIContent("Create symbols.zip"));
            }
            EditorGUI.indentLevel--;

            EditorGUI.EndDisabledGroup();

#endif // SYMBOLS_ZIP_AVAILABLE
        }

#endregion GUI


    }
}
