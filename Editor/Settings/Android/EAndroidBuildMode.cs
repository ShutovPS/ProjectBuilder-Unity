namespace Mobcast.Coffee.Build.Editor
{
    [System.Serializable]
    public enum EAndroidBuildMode
    {
        APK = 1 << 0,
        ANDROID_PROJECT = 1 << 1,
        GOOGLE_BUNDLE = 1 << 2,
    }
}
