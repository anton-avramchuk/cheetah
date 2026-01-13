using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cheetah.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ModuleDependencyCodeFixProvider)), Shared]
public class ModuleDependencyCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(
            DiagnosticDescriptors.MissingModuleDependency.Id,
            DiagnosticDescriptors.UnusedModuleDependency.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var classDeclaration = root.FindToken(diagnosticSpan.Start)
            .Parent?
            .AncestorsAndSelf()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault();

        if (classDeclaration is null)
            return;

        if (diagnostic.Id == DiagnosticDescriptors.MissingModuleDependency.Id)
        {
            // Single fix - add one missing attribute
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: $"Add [DependsOn(typeof({GetModuleTypeFromDiagnostic(diagnostic)}))]",
                    createChangedDocument: c => AddSingleDependsOnAttributeAsync(context.Document, classDeclaration, diagnostic, c),
                    equivalenceKey: nameof(ModuleDependencyCodeFixProvider) + ".AddSingle"),
                diagnostic);

            // Batch fix - add all missing attributes at once
            var allDiagnostics = context.Diagnostics
                .Where(d => d.Id == DiagnosticDescriptors.MissingModuleDependency.Id)
                .ToList();

            if (allDiagnostics.Count > 1)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: $"Add all {allDiagnostics.Count} missing [DependsOn] attributes",
                        createChangedDocument: c => AddAllDependsOnAttributesAsync(context.Document, classDeclaration, allDiagnostics, c),
                        equivalenceKey: nameof(ModuleDependencyCodeFixProvider) + ".AddAll"),
                    diagnostic);
            }
        }
        else if (diagnostic.Id == DiagnosticDescriptors.UnusedModuleDependency.Id)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Remove unused [DependsOn] attribute",
                    createChangedDocument: c => RemoveDependsOnAttributeAsync(context.Document, diagnostic, c),
                    equivalenceKey: nameof(ModuleDependencyCodeFixProvider) + ".RemoveDependsOn"),
                diagnostic);
        }
    }

    private async Task<Document> AddSingleDependsOnAttributeAsync(
        Document document,
        ClassDeclarationSyntax classDeclaration,
        Diagnostic diagnostic,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
            return document;

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
            return document;

        var moduleTypeName = GetModuleTypeFromDiagnostic(diagnostic);
        if (string.IsNullOrEmpty(moduleTypeName))
            return document;

        var compilation = semanticModel.Compilation;
        var moduleType = FindTypeInCompilation(compilation, moduleTypeName);
        if (moduleType is null)
            return document;

        var newClassDeclaration = AddDependsOnAttribute(classDeclaration, moduleType);
        var newRoot = root.ReplaceNode(classDeclaration, newClassDeclaration);

        return document.WithSyntaxRoot(newRoot);
    }

    private async Task<Document> AddAllDependsOnAttributesAsync(
        Document document,
        ClassDeclarationSyntax classDeclaration,
        List<Diagnostic> diagnostics,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
            return document;

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
            return document;

        var compilation = semanticModel.Compilation;
        var newClassDeclaration = classDeclaration;

        // Add all missing attributes
        foreach (var diagnostic in diagnostics)
        {
            var moduleTypeName = GetModuleTypeFromDiagnostic(diagnostic);
            if (string.IsNullOrEmpty(moduleTypeName))
                continue;

            var moduleType = FindTypeInCompilation(compilation, moduleTypeName);
            if (moduleType is null)
                continue;

            newClassDeclaration = AddDependsOnAttribute(newClassDeclaration, moduleType);
        }

        var newRoot = root.ReplaceNode(classDeclaration, newClassDeclaration);
        return document.WithSyntaxRoot(newRoot);
    }

    private ClassDeclarationSyntax AddDependsOnAttribute(
        ClassDeclarationSyntax classDeclaration,
        INamedTypeSymbol moduleType)
    {
        // Create attribute syntax: [DependsOn(typeof(ModuleName))]
        var attributeArgument = SyntaxFactory.AttributeArgument(
            SyntaxFactory.TypeOfExpression(
                SyntaxFactory.ParseTypeName(moduleType.ToDisplayString())));

        var attribute = SyntaxFactory.Attribute(
            SyntaxFactory.IdentifierName("DependsOn"),
            SyntaxFactory.AttributeArgumentList(
                SyntaxFactory.SingletonSeparatedList(attributeArgument)));

        var attributeList = SyntaxFactory.AttributeList(
            SyntaxFactory.SingletonSeparatedList(attribute))
            .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

        return classDeclaration.AddAttributeLists(attributeList);
    }

    private async Task<Document> RemoveDependsOnAttributeAsync(
        Document document,
        Diagnostic diagnostic,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
            return document;

        var attributeSyntax = root.FindNode(diagnostic.Location.SourceSpan)
            .AncestorsAndSelf()
            .OfType<AttributeSyntax>()
            .FirstOrDefault();

        if (attributeSyntax is null)
            return document;

        var attributeList = attributeSyntax.Parent as AttributeListSyntax;
        if (attributeList is null)
            return document;

        SyntaxNode? newRoot;

        if (attributeList.Attributes.Count == 1)
        {
            // Remove entire attribute list if it's the only attribute
            newRoot = root.RemoveNode(attributeList, SyntaxRemoveOptions.KeepNoTrivia);
        }
        else
        {
            // Remove only this attribute
            var newAttributeList = attributeList.RemoveNode(attributeSyntax, SyntaxRemoveOptions.KeepNoTrivia);
            newRoot = root.ReplaceNode(attributeList, newAttributeList!);
        }

        if (newRoot is null)
            return document;

        return document.WithSyntaxRoot(newRoot);
    }

    private string? GetModuleTypeFromDiagnostic(Diagnostic diagnostic)
    {
        // Extract module type name from diagnostic message
        // Message format: "Project references '{0}' but module '{1}' is missing [DependsOn(typeof({2}))]"
        var message = diagnostic.GetMessage();
        var startIndex = message.LastIndexOf("typeof(");
        if (startIndex == -1)
            return null;

        startIndex += 7; // length of "typeof("
        var endIndex = message.IndexOf(')', startIndex);
        if (endIndex == -1)
            return null;

        return message.Substring(startIndex, endIndex - startIndex);
    }

    private INamedTypeSymbol? FindTypeInCompilation(Compilation compilation, string typeName)
    {
        // Search in all referenced assemblies
        foreach (var reference in compilation.References)
        {
            var assembly = compilation.GetAssemblyOrModuleSymbol(reference) as IAssemblySymbol;
            if (assembly is null)
                continue;

            var type = FindTypeInNamespace(assembly.GlobalNamespace, typeName);
            if (type is not null)
                return type;
        }

        return null;
    }

    private INamedTypeSymbol? FindTypeInNamespace(INamespaceSymbol namespaceSymbol, string typeName)
    {
        foreach (var type in namespaceSymbol.GetTypeMembers())
        {
            if (type.Name == typeName)
                return type;
        }

        foreach (var nestedNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            var result = FindTypeInNamespace(nestedNamespace, typeName);
            if (result is not null)
                return result;
        }

        return null;
    }
}
