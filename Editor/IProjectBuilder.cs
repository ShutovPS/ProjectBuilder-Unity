using System;
using Mobcast.Coffee.Build.Editor;
using UnityEditor;
using UnityEngine;

namespace Mobcast.Coffee.Build
{
    public interface IProjectBuilder
    {
        /// <summary>Build Application.</summary>
        bool BuildApplication { get; }
        

        /// <summary>The currently active build target.</summary>
        BuildTarget ActualBuildTarget { get; }

        /// <summary>Build target group for this builder asset.</summary>
        BuildTargetGroup BuildTargetGroup { get; }
        
        
        /// <summary>The application identifier for the specified platform.</summary>
        string ApplicationIdentifier { get; }

        /// <summary>The name of your product.</summary>
        string ProductName { get; }
        
        
        /// <summary>Application bundle version shared.</summary>
        string Version { get; }

        /// <summary>Application bundle version code for Android and version number for iOS.</summary>
        int VersionCode { get; }
        

        /// <summary>Build root path.</summary>
        string BuildPath { get; }

        /// <summary>Build directory name.</summary>
        string BuildDirectoryName { get; }

        /// <summary>Builds file name.</summary>
        string BuildName { get; }
        

        /// <summary>Build AssetBundle.</summary>
        bool BuildAssetBundle { get; }

        /// <summary>Asset Bundles output path.</summary>
        string BundleOutputPath { get; }


        void ReadSettings();

        /// <summary>
        /// Define script symbol.
        /// </summary>
        bool DefineSymbol();

        /// <summary>
        /// PlayerSettingにビルド設定を反映します.
        /// </summary>
        void ApplySettings();

        /// <summary>
        /// アセットバンドルをビルドします.
        /// </summary>
        /// <returns>ビルドに成功していればtrueを、それ以外はfalseを返す.</returns>
        bool BuildAssetBundles();

        /// <summary>
        /// BuildPipelineによるビルドを実行します.
        /// </summary>
        /// <returns>ビルドに成功していればtrueを、それ以外はfalseを返す.</returns>
        /// <param name="autoRunPlayer">Build & Runモードでビルドします.</param>
        bool BuildPlayer(bool autoRunPlayer);
        
        
        IAndroidBuildSettings GetAndroidSettings { get; }
        IiOSBuildSettings GetiOSSettings { get; }
        IWebGLBuildSettings GetWebGLSettings { get; }
    }
}
