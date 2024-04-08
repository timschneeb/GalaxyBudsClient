using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GalaxyBudsClient.Generators.Enums;

public readonly struct AttributeTemplate(string name, IEnumerable<string> parameters)
{
    public string Name { get; } = name;
    public IEnumerable<string> Parameters { get; } = parameters;
}

public static class AttributeExtensions
{
    public static IEnumerable<string> GetUsingDirectivesInFileOfSymbols(this ISymbol symbol)
    {
        var node = symbol.Locations.FirstOrDefault()?.SourceTree?.GetRoot();

        if (node is not CompilationUnitSyntax root)
            throw new InvalidOperationException("The provided symbol does not have a valid location.");
   
        return root.Usings.Select(u => u.ToFullString());
    }
    
    public static List<AttributeTemplate> ParseAttributesForEnumMember(this ISymbol enumMember)
    {
        var syntax = enumMember.DeclaringSyntaxReferences.SingleOrDefault()?.GetSyntax();
        if(syntax is not EnumMemberDeclarationSyntax memberDeclSyntax)
            throw new InvalidOperationException("The provided member is not an enum member.");
        
        var attributeTemplates = new List<AttributeTemplate>();
        
        var attributeLists = memberDeclSyntax.AttributeLists;
        foreach (var list in attributeLists)
        {
            foreach (var attribute in list.Attributes)
            {
                var parameters = new List<string>();
                var attributeTemplate = new AttributeTemplate(attribute.Name.ToFullString(), parameters);

                if (attribute.ArgumentList?.Arguments == null)
                {
                    attributeTemplates.Add(attributeTemplate);
                    continue;
                }
            
                foreach (var argument in attribute.ArgumentList.Arguments)
                {
                    /*
                     * We support Expressions that are either a LiteralExpressionSyntax or a MemberAccessExpressionSyntax.
                     * In both cases, just call ToString() on the expression to get the parameter value.
                     * For MemberAccessExpressionSyntax, appropriate using directives need to be added to the generated file if the expression is not fully qualified.
                     */
                    parameters.Add(argument.Expression.ToString());
                }
            
                attributeTemplates.Add(attributeTemplate);
            }
        }
   
        return attributeTemplates;
    }
}