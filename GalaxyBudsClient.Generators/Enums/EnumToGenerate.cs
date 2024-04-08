using System.Collections.Generic;
using System.Linq;

namespace GalaxyBudsClient.Generators.Enums;

public readonly record struct EnumToGenerate
{
    public readonly string Name;
    public readonly string FullyQualifiedName;
    public readonly string Namespace;
    public readonly bool IsPublic;
    public readonly bool HasFlags;
    public readonly string UnderlyingType;
    public readonly IEnumerable<string> UsingDirectives;
    public readonly IEnumerable<string> UsedAttributes;

    /// <summary>
    /// Key is the enum name.
    /// </summary>
    public readonly (string Key, EnumValueOption Value)[] Names;

    public string ExtName => Name + "Extensions";

    public EnumToGenerate(
        string name,
        string ns,
        string fullyQualifiedName,
        string underlyingType,
        bool isPublic,
        List<(string, EnumValueOption)> names,
        bool hasFlags,
        IEnumerable<string> usingDirectives)
    {
        Name = name;
        Namespace = ns;
        UnderlyingType = underlyingType;
        Names = names.ToArray();
        HasFlags = hasFlags;
        IsPublic = isPublic;
        FullyQualifiedName = fullyQualifiedName;
        UsingDirectives = usingDirectives;

        var usedAttrs = new List<string>();
        foreach (var (_, opts) in names)
        {
            usedAttrs.AddRange(opts.AttributeTemplates.Select(x => x.Name));
        }

        UsedAttributes = usedAttrs.Distinct();
    }
}