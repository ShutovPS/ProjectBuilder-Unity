using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Mobcast.Coffee.Build.Editor
{
    [System.Serializable]
    public class AndroidKeystoreBuildPartSettings : IAndroidKeystoreBuildPartSettings
    {


#region KEYSTORE

        /// <summary>Enable application signing with a custom keystore.</summary>
        [Tooltip("Enable application signing with a custom keystore.")]
        [SerializeField] protected bool _useCustomKeystore = false;

        /// <summary>Keystore file path.</summary>
        [Tooltip("Keystore file path.")]
        [SerializeField] protected string _keystoreFile = null;

        /// <summary>Keystore password.</summary>
        [Tooltip("Keystore password.")]
        [SerializeField] protected string _keystorePassword = null;

        /// <summary>Keystore alias name.</summary>
        [Tooltip("Keystore alias name.")]
        [SerializeField] protected string _keystoreAliasName = null;

        /// <summary>Keystore alias password.</summary>
        [Tooltip("Keystore alias password.")]
        [SerializeField] protected string _keystoreAliasPassword = null;

#endregion KEYSTORE


        [NonSerialized] protected bool _showPasswords = false;


        public bool UseCustomKeystore => _useCustomKeystore;

        public string KeystoreFile => _keystoreFile;

        public string KeystorePassword => _keystorePassword;

        public string KeystoreAliasName => _keystoreAliasName;

        public string KeystoreAliasPassword => _keystoreAliasPassword;


        public virtual void ReadSettings(IAndroidBuildSettings buildSettings)
        {
            _useCustomKeystore = PlayerSettings.Android.useCustomKeystore;

            _keystoreFile = PlayerSettings.Android.keystoreName.Replace("\\", "/")
                .Replace(ProjectBuilderUtil.projectDir + "/", "");
            _keystorePassword = PlayerSettings.Android.keystorePass;
            _keystoreAliasName = PlayerSettings.Android.keyaliasName;
            _keystoreAliasPassword = PlayerSettings.Android.keyaliasPass;
        }

        public virtual void ApplySettings(IAndroidBuildSettings buildSettings)
        {
            PlayerSettings.Android.useCustomKeystore = _useCustomKeystore;

            PlayerSettings.Android.keystoreName = _keystoreFile;
            PlayerSettings.Android.keystorePass = _keystorePassword;
            PlayerSettings.Android.keyaliasName = _keystoreAliasName;
            PlayerSettings.Android.keyaliasPass = _keystoreAliasPassword;
        }


#region GUI

        public virtual void DrawSetting(SerializedProperty settings, IAndroidBuildSettings buildSettings)
        {
            EditorGUILayout.LabelField("Keystore", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            {
                var useCustomKeystore = settings.FindPropertyRelative(nameof(_useCustomKeystore));
                EditorGUILayout.PropertyField(useCustomKeystore, new GUIContent("Use custom keystore"));

                EditorGUI.BeginDisabledGroup(!useCustomKeystore.boolValue);

                var keystoreFile = settings.FindPropertyRelative(nameof(_keystoreFile));
                EditorGUIExtensions.FilePathField(keystoreFile, "Select keystore file.", "", "");

                DrawPassword(settings, nameof(_keystorePassword), new GUIContent("Keystore Password"));

                var keystoreAliasName = settings.FindPropertyRelative(nameof(_keystoreAliasName));
                EditorGUILayout.PropertyField(keystoreAliasName, new GUIContent("Alias"));

                DrawPassword(settings, nameof(_keystoreAliasPassword), new GUIContent("Alias Password"));

                EditorGUI.EndDisabledGroup();

                _showPasswords = EditorGUILayout.Toggle(new GUIContent("Hide Passwords"), _showPasswords);
            }
            EditorGUI.indentLevel--;
        }

        protected virtual void DrawPassword(SerializedProperty settings, string propertyId, GUIContent label = null)
        {
            var property = settings.FindPropertyRelative(propertyId);

            if (label == null)
            {
                label = new GUIContent(propertyId);
            }

            if (_showPasswords)
            {
                property.stringValue = EditorGUILayout.PasswordField(label, property.stringValue);
            }
            else
            {
                EditorGUILayout.PropertyField(property, label);
            }
        }

#endregion GUI


    }
}
