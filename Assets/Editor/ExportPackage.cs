using UnityEditor;

namespace Mobcast.Coffee.Build {
	public static class ExportPackage {
		private const string kPackageName = "Unity-ProjectBuilder";
		private const string kPackageVersion = "1.0.2";

		private static readonly string[] kAssetPathes = {
			"Assets/Unity-ProjectBuilder",
		};

		[MenuItem("Export Package/" + kPackageName)]
	//		[InitializeOnLoadMethod]
		private static void Export() {
			if (EditorApplication.isPlayingOrWillChangePlaymode) {
				return;
			}

			string fileName = $"{kPackageName}-{kPackageVersion}.unitypackage".ToLower();
			AssetDatabase.ExportPackage(kAssetPathes, fileName,
				ExportPackageOptions.Recurse | ExportPackageOptions.Default);
			
			string latestFileName = $"{kPackageName}-latest.unitypackage".ToLower();
			AssetDatabase.ExportPackage(kAssetPathes, latestFileName,
				ExportPackageOptions.Recurse | ExportPackageOptions.Default);
			
			UnityEngine.Debug.Log("Export successfully : " + kPackageName);
		}
	}
}
