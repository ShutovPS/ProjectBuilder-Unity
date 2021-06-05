using System;
using UnityEngine;

namespace Mobcast.Coffee.Build.Editor
{
    public interface IiOSLanguagesBuildPartSettings : IiOSBuildPartSettings
    {
        /// <summary>Support languages. If you have multiple definitions, separate with a semicolon(;)</summary>
        string Languages { get; }
    }
}
