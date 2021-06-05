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
        

        /// <summary>ビルドに利用するターゲット.</summary>
        BuildTarget ActualBuildTarget { get; }

        /// <summary>Build target group for this builder asset.</summary>
        BuildTargetGroup BuildTargetGroup { get; }
        
        
        /// <summary>プロダクトのバンドル識別子を指定します.</summary>
        string ApplicationIdentifier { get; }

        /// <summary>端末に表示されるプロダクト名を指定します.</summary>
        string ProductName { get; }
        
        
        /// <summary>アプリのバージョンを指定します.</summary>
        string Version { get; }

        /// <summary>バンドルコードを指定します.Androidの場合はVersionCode, iOSの場合はBuildNumberに相当します.この値は、リリース毎に更新する必要があります.</summary>
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
