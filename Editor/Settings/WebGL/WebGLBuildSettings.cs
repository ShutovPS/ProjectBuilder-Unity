using UnityEditor;
using UnityEngine;

namespace Mobcast.Coffee.Build.Editor
{
    [System.Serializable]
    public class WebGLBuildSettings : IWebGLBuildSettings
    {
        public BuildTarget GetBuildTarget
        {
            get { return BuildTarget.WebGL; }
        }

        public Texture GetIcon
        {
            get { return EditorGUIUtility.FindTexture("BuildSettings.WebGL.Small"); }
        }

        public virtual void ReadSettings(IProjectBuilder builder)
        {
        }

        public virtual void ApplySettings(IProjectBuilder builder)
        {
        }

        public virtual void DrawSetting(SerializedProperty settings)
        {
        }
    }
}
