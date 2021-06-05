using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Mobcast.Coffee.Build.Editor
{
    public static class BuildPathUtils
    {
        public static string GetVersionCodeLong(IProjectBuilder projectBuilder)
        {
            if (System.Version.TryParse(projectBuilder.Version, out var v))
            {
                return $"{v.Major}{v.Minor:00}{v.Build:00}{projectBuilder.VersionCode:00}";
            }

            return projectBuilder.VersionCode.ToString();
        }

        public static string GetBuildOutputDirectoryPath(IProjectBuilder projectBuilder)
        {
            string dirPath = projectBuilder.BuildPath;
            dirPath = ConvertPath(dirPath, projectBuilder);
            
            string dirName = projectBuilder.BuildDirectoryName;
            dirName = ConvertPath(dirName, projectBuilder);

            return Path.Combine(dirPath, dirName);
        }

        public static string GetBuildOutputFileName(IProjectBuilder projectBuilder)
        {
            string fileName = projectBuilder.BuildName;

            fileName = ConvertPath(fileName, projectBuilder);

            return fileName;
        }

        public static string GetOutputPath(IProjectBuilder projectBuilder)
        {
            string dirPath = GetBuildOutputDirectoryPath(projectBuilder);
            string fileName = GetBuildOutputFileName(projectBuilder);

            string buildPath = Path.Combine(dirPath, fileName);
            return buildPath;
        }

        private static string ConvertPath(string inPath, IProjectBuilder projectBuilder)
        {
            var date = DateTime.Now;

            string outPath = inPath;
            outPath = outPath.Replace("$IDENTIFIER", projectBuilder.ApplicationIdentifier);
            outPath = outPath.Replace("$NAME", GetProductName(projectBuilder));
            
            outPath = outPath.Replace("$PLATFORM", ConvertBuildTargetToString(projectBuilder));
            
            outPath = outPath.Replace("$VERSION_CODE_LONG", GetVersionCodeLong(projectBuilder));
            outPath = outPath.Replace("$VERSION_CODE", projectBuilder.VersionCode.ToString());
            outPath = outPath.Replace("$VERSION", projectBuilder.Version);
            
            outPath = outPath.Replace("$DATE_YEAR", date.Year.ToString());
            outPath = outPath.Replace("$DATE_MONTH", date.Month.ToString());
            outPath = outPath.Replace("$DATE_DAY", date.Day.ToString());
            outPath = outPath.Replace("$DATE", date.ToString("yyyy-M-d"));
            
            outPath = outPath.Replace("$TIME_HOUR", date.ToString("HH"));
            outPath = outPath.Replace("$TIME_MINUTES", date.ToString("mm"));
            outPath = outPath.Replace("$TIME_SECONDS", date.ToString("ss"));
            outPath = outPath.Replace("$TIME", date.ToString("HH-mm-ss"));
            
            outPath = outPath.Replace("$EXECUTABLE", GetBuildTargetExecutable(projectBuilder));

            return outPath;
        }

        private static string ConvertBuildTargetToString(IProjectBuilder projectBuilder)
        {
            var buildTarget = projectBuilder.ActualBuildTarget;
            return ConvertBuildTargetToString(buildTarget);
        }

        public static string ConvertBuildTargetToString(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.StandaloneOSX:
                    return "OSX";
                case BuildTarget.StandaloneWindows:
                    return "Windows32";
                case BuildTarget.StandaloneWindows64:
                    return "Windows64";
                case BuildTarget.StandaloneLinux64:
                    return "Linux";
            }

            return target.ToString();
        }

        private static string GetProductName(IProjectBuilder projectBuilder)
        {
            string productName = projectBuilder.ProductName;

            return GetProductName(productName);
        }

        private static string GetProductName(string productName)
        {
            return productName
                    .Replace(' ', '_')
                    .Replace('/', '_')
                    .Replace('\\', '_')
                    .Replace(':', '_')
                    .Replace('*', '_')
                    .Replace('?', '_')
                    .Replace('"', '_')
                    .Replace('<', '_')
                    .Replace('>', '_')
                    .Replace('|', '_')
                ;
        }

        private static string GetBuildTargetExecutable(IProjectBuilder projectBuilder)
        {
            var buildTarget = projectBuilder.ActualBuildTarget;

            switch (buildTarget)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return ".exe";

                case BuildTarget.StandaloneLinux64:
                    return ".x86_64";

                case BuildTarget.StandaloneOSX:
                    return "";

                case BuildTarget.iOS:
                    return ".ipa";

                case BuildTarget.Android:
                    var buildMode = projectBuilder.GetAndroidSettings.TargetSettings.BuildMode;
                    switch (buildMode)
                    {
                        case EAndroidBuildMode.APK:
                            return ".apk";
                        case EAndroidBuildMode.GOOGLE_BUNDLE:
                            return ".aab";
                    }

                    break;

                case BuildTarget.WebGL:
                    return "";
            }

            return "";
        }
    }
}
