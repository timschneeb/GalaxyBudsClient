using System.Collections.Generic;

namespace GalaxyBudsClient.Generators.Enums;

public readonly struct EnumValueOption(IReadOnlyList<AttributeTemplate> attributeTemplates)
{
    public IReadOnlyList<AttributeTemplate> AttributeTemplates { get; } = attributeTemplates;
}
