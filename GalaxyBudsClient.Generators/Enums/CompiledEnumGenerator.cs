using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace GalaxyBudsClient.Generators.Enums;

[Generator]
public class CompiledEnumGenerator : IIncrementalGenerator
{
    private const string DisplayAttribute = "System.ComponentModel.DataAnnotations.DisplayAttribute";
    private const string DescriptionAttribute = "System.ComponentModel.DescriptionAttribute";
    private const string EnumExtensionsAttribute = "GalaxyBudsClient.Generated.Model.Attributes.CompiledEnumAttribute";
    private const string FlagsAttribute = "System.FlagsAttribute";
    
    public const string InitialExtraction = nameof(InitialExtraction);
    public const string RemovingNulls = nameof(RemovingNulls);
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "CompiledEnumAttribute.g.cs", SourceText.From(RootSourceGenerationHelper.GenerateAttribute(), Encoding.UTF8)));

        var enumsToGenerate = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                EnumExtensionsAttribute,
                predicate: (node, _) => node is EnumDeclarationSyntax,
                transform: GetTypeToGenerate)
            .WithTrackingName(InitialExtraction)
            .Where(static m => m is not null)
            .WithTrackingName(RemovingNulls);

        
        context.RegisterSourceOutput(enumsToGenerate,
            static (spc, enumToGenerate) => Execute(in enumToGenerate, spc));
        
        context.RegisterSourceOutput(enumsToGenerate.Collect(),
            static (spc, enumToGenerate) => ExecuteRoot(in enumToGenerate, spc));
        
    }

    static void ExecuteRoot(in ImmutableArray<EnumToGenerate?> enumToGenerate, SourceProductionContext context)
    {
        if (enumToGenerate is { } eg)
        {
            var result = RootSourceGenerationHelper.GenerateClass(eg);
            context.AddSource("CompiledEnums.g.cs", SourceText.From(result, Encoding.UTF8));    
        }
    }
    
    static void Execute(in EnumToGenerate? enumToGenerate, SourceProductionContext context)
    {
        try
        {
            if (enumToGenerate is { } eg)
            {
                var result = SourceGenerationHelper.GenerateExtensionClass(in eg);
                context.AddSource(eg.Name + "_CompiledEnumExtensions.g.cs", SourceText.From(result, Encoding.UTF8));
                var bindingSource = SourceGenerationHelper.GenerateBindingSource(in eg);
                context.AddSource(eg.Name + "_BindingSource.g.cs", SourceText.From(bindingSource, Encoding.UTF8));
            }
        }
        catch (Exception e)
        {
                            
#pragma warning disable RS1035
            File.AppendAllText("/home/tim/log", $"{e}\n");
#pragma warning restore RS1035
            throw;
        }
    }

    static EnumToGenerate? GetTypeToGenerate(GeneratorAttributeSyntaxContext context, CancellationToken ct)
    {
        if (context.TargetSymbol is not INamedTypeSymbol enumSymbol)
        {
            // nothing to do if this type isn't available
            return null;
        }

        ct.ThrowIfCancellationRequested();

        var nameSpace = enumSymbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : enumSymbol.ContainingNamespace.ToString();
        var hasFlags = false;

        foreach (var attributeData in enumSymbol.GetAttributes())
        {
            if (attributeData.AttributeClass?.Name is "FlagsAttribute" or "Flags" &&
                attributeData.AttributeClass.ToDisplayString() == FlagsAttribute)
            {
                hasFlags = true;
            }
        }

        var fullyQualifiedName = enumSymbol.ToString();
        var underlyingType = enumSymbol.EnumUnderlyingType?.ToString() ?? "int";

        var enumMembers = enumSymbol.GetMembers();
        var members = new List<(string, EnumValueOption)>(enumMembers.Length);
        
        foreach (var member in enumMembers)
        {
            if (member is not IFieldSymbol field || field.ConstantValue is null)
                continue;
            
            var attributeTemplates = member.ParseAttributesForEnumMember();
            foreach (var template in attributeTemplates)
            {
#pragma warning disable RS1035
                File.AppendAllText("/home/tim/log", $"{template.Name} - {string.Join(",", template.Parameters)}\n");
#pragma warning restore RS1035
            }
            
            members.Add((member.Name, new EnumValueOption(attributeTemplates)));
        }

        return new EnumToGenerate(
            name: enumSymbol.Name,
            fullyQualifiedName: fullyQualifiedName,
            ns: nameSpace,
            underlyingType: underlyingType,
            isPublic: enumSymbol.DeclaredAccessibility == Accessibility.Public,
            hasFlags: hasFlags,
            names: members,
            usingDirectives: enumSymbol.GetUsingDirectivesInFileOfSymbols());
    }
}
