using System;
using UnityEngine;

namespace Mobcast.Coffee.Build.Editor
{
    public interface IiOSEntitlementsBuildPartSettings : IiOSBuildPartSettings
    {
        /// <summary>Entitlements file(*.entitlement).</summary>
        string EntitlementsFile { get; }
    }
}
