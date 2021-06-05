using System;
using UnityEngine;

namespace Mobcast.Coffee.Build.Editor
{
    public interface IiOSSigningBuildPartSettings : IiOSBuildPartSettings
    {
        /// <summary>Enable automatically sign.</summary>
        bool AutomaticallySign { get; }

        /// <summary>Developer Team Id.</summary>
        string DeveloperTeamId { get; }

        /// <summary>Code Sign Identifier.</summary>
        string CodeSignIdentity { get; }


        /// <summary>Provisioning Profile Id. For example: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx</summary>
        string ProfileId { get; }


        /// <summary>Provisioning Profile Specifier. For example: com campany app_name</summary>
        string ProfileSpecifier { get; }
    }
}
