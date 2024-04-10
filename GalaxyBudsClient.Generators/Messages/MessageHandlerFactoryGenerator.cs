using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using GalaxyBudsClient.Generators.Enums;
using GalaxyBudsClient.Generators.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace GalaxyBudsClient.Generators.Messages;

[Generator]
public class MessageHandlerFactoryGenerator : IIncrementalGenerator
{
    private const string DecoderBaseClassName = "BaseMessageDecoder";
    private const string EncoderBaseClassName = "BaseMessageEncoder";
    private const string DecoderNamespace = "GalaxyBudsClient.Message.Decoder";
    private const string EncoderNamespace = "GalaxyBudsClient.Message.Encoder";
    
    private const string MessageEncoderAttribute = "GalaxyBudsClient.Generated.Model.Attributes.MessageEncoderAttribute";
    private const string MessageDecoderAttribute = "GalaxyBudsClient.Generated.Model.Attributes.MessageDecoderAttribute";
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        /*var handlers = context.CompilationProvider
            .SelectMany((compilation, _) =>
                compilation.SyntaxTrees.Select(syntaxTree => compilation.GetSemanticModel(syntaxTree)))
            .SelectMany(
                (semanticModel, _) => semanticModel
                    .SyntaxTree
                    .GetRoot()
                    .DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .Select(classDeclarationSyntax => semanticModel.GetDeclaredSymbol(classDeclarationSyntax))
                    .OfType<INamedTypeSymbol>()
                    .Where(x => x.BaseType?.Name is DecoderBaseClassName or EncoderBaseClassName &&
                                x.ContainingNamespace.ToString() is DecoderNamespace or EncoderNamespace));*/
        
        
        var decodersToGenerate = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                MessageDecoderAttribute,
                predicate: (node, _) => node is ClassDeclarationSyntax,
                transform: GetDecoderTypeToGenerate)
            .Where(static m => m is not null);

        var encodersToGenerate = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                MessageEncoderAttribute,
                predicate: (node, _) => node is ClassDeclarationSyntax,
                transform: GetEncoderTypeToGenerate)
            .Where(static m => m is not null);

        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "MessageDecoderAttribute.g.cs", SourceText.From(CreateAttribute("Decoder"), Encoding.UTF8)));
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "MessageEncoderAttribute.g.cs", SourceText.From(CreateAttribute("Encoder"), Encoding.UTF8)));
        
        context.RegisterSourceOutput(decodersToGenerate.Collect(), Execute);
        context.RegisterSourceOutput(encodersToGenerate.Collect(), Execute);
    }

    private static HandlerToGenerate? GetDecoderTypeToGenerate(GeneratorAttributeSyntaxContext context, CancellationToken ct) 
        => GetTypeToGenerate(context, ct, HandlerType.Decoder);
    
    private static HandlerToGenerate? GetEncoderTypeToGenerate(GeneratorAttributeSyntaxContext context, CancellationToken ct) 
        => GetTypeToGenerate(context, ct, HandlerType.Encoder);

    private static HandlerToGenerate? GetTypeToGenerate(GeneratorAttributeSyntaxContext context, CancellationToken ct, HandlerType type)
    {
        if (context.TargetSymbol is not INamedTypeSymbol classSymbol)
        {
            // nothing to do if this type isn't available
            return null;
        }

        ct.ThrowIfCancellationRequested();
        
        return new HandlerToGenerate(
            FullyQualifiedName: classSymbol.ToString(),
            MessageId: ParseMsgIdForClass(classSymbol),
            Type: type);
    }

    private static string ParseMsgIdForClass(ISymbol classSymbol)
    {
        var syntax = classSymbol.DeclaringSyntaxReferences.SingleOrDefault()?.GetSyntax();
        if(syntax is not ClassDeclarationSyntax memberDeclSyntax)
            throw new InvalidOperationException("The provided symbol is not a class declaration.");

        var attributeLists = memberDeclSyntax.AttributeLists;
        foreach (var list in attributeLists)
        {
            foreach (var attribute in list.Attributes)
            {
                if(attribute.Name.ToFullString() is not ("MessageDecoder" or "MessageEncoder"))
                    continue;
        
                /*
                 * We support Expressions that are either a LiteralExpressionSyntax or a MemberAccessExpressionSyntax.
                 * In both cases, just call ToString() on the expression to get the parameter value.
                 * For MemberAccessExpressionSyntax, appropriate using directives need to be added to the generated file if the expression is not fully qualified.
                 */
                var expression = attribute.ArgumentList?.Arguments.FirstOrDefault()?.Expression;
                if (expression != null)
                    return expression.ToString();
            }
        }

        throw new InvalidOperationException("MessageId not found in attribute constructor");
    }
    
    private static void Execute(SourceProductionContext context, ImmutableArray<HandlerToGenerate?> handlersToGenerate)
    {
        var gen = new CodeGenerator();
        gen.AppendLines("""
                        // <auto-generated/>
                        #nullable enable
                        
                        namespace GalaxyBudsClient.Message;
                        """);
        
        gen.EnterScope("public partial class SppMessage");
        
        var type = handlersToGenerate.FirstOrDefault()?.Type switch
        {
            HandlerType.Decoder => "Decoder",
            HandlerType.Encoder => "Encoder",
            _ => null
        };
        
        if (type is null)
            return;
        
        gen.EnterScope($"private static GalaxyBudsClient.Message.{type}.BaseMessage{type}? CreateUninitialized{type}(MsgIds msgId)");
        gen.EnterScope("return msgId switch");
        
        foreach (var handler in handlersToGenerate.Cast<HandlerToGenerate>())
        {
            gen.AppendLine($"{handler.MessageId} => new {handler.FullyQualifiedName}(),");
        }

        gen.AppendLine("_ => null");
        
        gen.LeaveScope(";");
        
        gen.LeaveScope();
        gen.LeaveScope();
        
        context.AddSource($"SppMessage_{type}.g.cs", SourceText.From(gen.ToString(), Encoding.UTF8));
    }

    private string CreateAttribute(string type) =>
        $$"""
        // <auto-generated/>
        #pragma warning disable CS9113
        
        namespace GalaxyBudsClient.Generated.Model.Attributes;

        [global::System.AttributeUsage(global::System.AttributeTargets.Class)]
        public class Message{{type}}Attribute(global::GalaxyBudsClient.Message.MsgIds msgId) : global::System.Attribute {}
        """;
}