using System;
using UnityEditor;
using UnityEngine;

namespace Mobcast.Coffee.Build.Editor
{
    public interface IAndroidKeystoreBuildPartSettings : IAndroidBuildPartSettings
    {
        /// <summary>Enable application signing with a custom keystore.</summary>
        bool UseCustomKeystore { get; }

        /// <summary>Keystore file path.</summary
        string KeystoreFile { get; }

        /// <summary>Keystore password.</summary>
        string KeystorePassword { get; }

        /// <summary>Keystore alias name.</summary>
        string KeystoreAliasName { get; }

        /// <summary>Keystore alias password.</summary>
        string KeystoreAliasPassword { get; }
    }
}
