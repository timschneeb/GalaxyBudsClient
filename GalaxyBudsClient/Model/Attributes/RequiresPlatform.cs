using System;
using GalaxyBudsClient.Platform;

namespace GalaxyBudsClient.Model.Attributes;

public class RequiresPlatform(PlatformUtils.Platforms platform, int minBuild = -1) : Attribute
{
    public bool IsConditionMet => PlatformUtils.Platform == platform && 
                                  (minBuild == -1 || Environment.OSVersion.Version.Build >= minBuild);
}