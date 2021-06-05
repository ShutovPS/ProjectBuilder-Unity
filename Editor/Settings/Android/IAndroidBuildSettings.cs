using System;
using UnityEditor;
using UnityEngine;

namespace Mobcast.Coffee.Build.Editor
{
    public interface IAndroidBuildSettings : IBuildSettings
    {
        AndroidKeystoreBuildPartSettings KeystoreSettings { get; }
        
         AndroidTargetBuildPartSettings TargetSettings { get; }
        
        AndroidSymbolsZipBuildPartSettings SymbolsZipSettings { get; }
    }
}
