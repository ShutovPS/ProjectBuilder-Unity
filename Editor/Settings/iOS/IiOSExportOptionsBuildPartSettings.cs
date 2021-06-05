using System;
using UnityEngine;

namespace Mobcast.Coffee.Build.Editor
{
    public interface IiOSExportOptionsBuildPartSettings : IiOSBuildPartSettings
    {
        /// <summary>Generate exportOptions.plist automatically for xcodebuild (XCode7 and later).</summary>
        bool GenerateExportOptionPlist { get; }

        /// <summary>The method of distribution, which can be set as any of the following: app-store, ad-hoc, package, enterprise, development, developer-id.</summary>
        string ExportMethod { get; }

        /// <summary>Option to include Bitcode.</summary>
        bool UploadBitcode { get; }

        /// <summary>Option to include symbols in the generated ipa file.</summary>
        bool UploadSymbols { get; }
    }
}
