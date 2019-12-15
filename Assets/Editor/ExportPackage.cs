using UnityEditor;

namespace Mobcast.Coffee.Build {
	public static class ExportPackage {
		private const string kPackageName = "Unity-ProjectBuilder";
		private const string kPackageVersion = "1.0.1";

		private static readonly string[] kAssetPathes = {
			"Assets/Unity-ProjectBuilder",
		};

		[MenuItem("Export Package/" + kPackageName)]
//		[InitializeOnLoadMethod]
		private static void Export() {
			if (EditorApplication.isPlayingOrWillChangePlaymode) {
				return;
			}

			AssetDatabase.ExportPackage(kAssetPathes, $"{kPackageName}-{kPackageVersion}.unitypackage".ToLower(),
			                            ExportPackageOptions.Recurse | ExportPackageOptions.Default);
			
			UnityEngine.Debug.Log("Export successfully : " + kPackageName);
		}
	}
}
