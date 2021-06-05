using System;
using System.IO;
using UnityEditor;

namespace Mobcast.Coffee.Build.Editor
{
    /// <summary>
    /// BrojectBuilder共用クラス.
    /// </summary>
    internal static class ExcludeBuildDirectoriesUtil
    {
        private const string EXCLUDE_BUILD_DIR = "__ExcludeBuild";


        public static void ExcludeDirectories(params string[] dirs)
        {
            foreach (string dir in dirs)
            {
                ExcludeDirectory(dir);
            }
        }

        public static void ExcludeDirectory(string dir)
        {
            var d = new DirectoryInfo(dir);

            if (!d.Exists)
            {
                return;
            }

            if (!Directory.Exists(EXCLUDE_BUILD_DIR))
            {
                Directory.CreateDirectory(EXCLUDE_BUILD_DIR);
            }

            MoveDirectory(d.FullName, EXCLUDE_BUILD_DIR + "/" + dir.Replace("\\", "/").Replace("/", "~~"));

            AssetDatabase.Refresh();
        }

        /// <summary>ビルド隔離ディレクトリを戻します.</summary>
        [InitializeOnLoadMethod]
        public static void RevertExcludedDirectory()
        {
            var exDir = new DirectoryInfo(EXCLUDE_BUILD_DIR);
            if (!exDir.Exists)
            {
                return;
            }

            foreach (var d in exDir.GetDirectories())
            {
                MoveDirectory(d.FullName, d.Name.Replace("~~", "/"));
            }

            foreach (var f in exDir.GetFiles())
            {
                f.Delete();
            }

            exDir.Delete();
            AssetDatabase.Refresh();
        }

        /// <summary>ディレクトリをmetaファイルごと移動させます.</summary>
        private static void MoveDirectory(string from, string to)
        {
            const string metaExtension = ".meta";
            
            Directory.Move(from, to);

            if (File.Exists(from + metaExtension))
            {
                File.Move(from + metaExtension, to + metaExtension);
            }
        }
    }
}
