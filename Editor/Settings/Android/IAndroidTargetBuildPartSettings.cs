using System;
using UnityEditor;
using UnityEngine;

namespace Mobcast.Coffee.Build.Editor
{
    public interface IAndroidTargetBuildPartSettings : IAndroidBuildPartSettings
    {
        EScriptingBackend ScriptingBackend { get; }

        EAndroidArchitecture TargetArchitecture  { get; }

        EAndroidBuildMode BuildMode { get; }
    }
}
