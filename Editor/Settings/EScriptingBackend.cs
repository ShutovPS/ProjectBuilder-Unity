using UnityEditor;

namespace Mobcast.Coffee.Build.Editor
{
    [System.Serializable]
    public enum EScriptingBackend
    {
        Mono2x = ScriptingImplementation.Mono2x,
        IL2CPP = ScriptingImplementation.IL2CPP,
    }
}
