using UnityEditor;
using UnityEngine;

namespace Mobcast.Coffee.Build.Editor
{
    /// <summary>
    /// Build target settings interface.
    /// </summary>
    public interface IBuildPartSettings<T> where T : IBuildSettings
    {
        /// <summary>
        /// </summary>
        void ReadSettings(T builder);

        /// <summary>
        /// On Applies the settings.
        /// </summary>
        void ApplySettings(T builder);

        /// <summary>
        /// Draws the setting.
        /// </summary>
        void DrawSetting(SerializedProperty settings, T builder);
    }
}
