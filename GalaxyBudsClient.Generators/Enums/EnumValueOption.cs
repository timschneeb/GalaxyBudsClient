using System;

namespace GalaxyBudsClient.Generators.Enums;

public readonly struct EnumValueOption(string? displayName, bool isDisplayNameTheFirstPresence)
    : IEquatable<EnumValueOption>
{
    /// <summary>
    /// Custom name set by the <c>[Display(Name)]</c> attribute.
    /// </summary>
    public string? DisplayName { get; } = displayName;

    public bool IsDisplayNameTheFirstPresence { get; } = isDisplayNameTheFirstPresence;

    public bool Equals(EnumValueOption other)
    {
        return DisplayName == other.DisplayName &&
               IsDisplayNameTheFirstPresence == other.IsDisplayNameTheFirstPresence;
    }
}
