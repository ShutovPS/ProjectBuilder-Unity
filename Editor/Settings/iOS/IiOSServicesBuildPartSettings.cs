using System;
using UnityEngine;

namespace Mobcast.Coffee.Build.Editor
{
    public interface IiOSServicesBuildPartSettings : IiOSBuildPartSettings
    {
        /// <summary>Apple services. If you have multiple definitions, separate with a semicolon(;)</summary>
        string Services { get; }
    }
}
