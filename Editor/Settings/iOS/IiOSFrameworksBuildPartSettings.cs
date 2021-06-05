using System;
using UnityEngine;

namespace Mobcast.Coffee.Build.Editor
{
    public interface IiOSFrameworksBuildPartSettings : IiOSBuildPartSettings
    {
        /// <summary>Additional frameworks. If you have multiple definitions, separate with a semicolon(;)</summary>
        string Frameworks { get; }
    }
}
