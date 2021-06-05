using System;
using UnityEngine;

namespace Mobcast.Coffee.Build.Editor
{
    public interface IiOSBuildSettings : IBuildSettings
    {
        IiOSLanguagesBuildPartSettings LanguagesSettings { get; }
        IiOSFrameworksBuildPartSettings FrameworksSettings { get; }
        IiOSServicesBuildPartSettings ServicesSettings { get; }
        IiOSEntitlementsBuildPartSettings EntitlementsSettings { get; }
        IiOSSigningBuildPartSettings SigningSettings { get; }
        IiOSExportOptionsBuildPartSettings ExportOptionsSettings { get; }
    }
}
