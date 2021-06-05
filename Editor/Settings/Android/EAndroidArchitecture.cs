using System;
using UnityEditor;

namespace Mobcast.Coffee.Build.Editor
{
    [System.Serializable] [Flags]
    public enum EAndroidArchitecture : uint
    {
        None = AndroidArchitecture.None,
        
        ARMv7 = AndroidArchitecture.ARMv7,
        ARM64 = AndroidArchitecture.ARM64,
        
        All = AndroidArchitecture.All,
    }
}