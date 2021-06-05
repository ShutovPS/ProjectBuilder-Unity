using UnityEditor;
using UnityEngine;

namespace Mobcast.Coffee.Build.Editor
{
    /// <summary>
    /// Build target settings interface.
    /// </summary>
    public interface IBuildSettings
    {
        /// <summary>
        /// Build target.
        /// </summary>
        BuildTarget GetBuildTarget { get; }

        /// <summary>
        /// Icon for build target.
        /// </summary>
        Texture GetIcon { get; }

        
        /// <summary>
        /// </summary>
        void ReadSettings(IProjectBuilder builder);

        /// <summary>
        /// On Applies the settings.
        /// </summary>
        void ApplySettings(IProjectBuilder builder);

        /// <summary>
        /// Draws the setting.
        /// </summary>
        void DrawSetting(SerializedProperty settings);
    }
}
