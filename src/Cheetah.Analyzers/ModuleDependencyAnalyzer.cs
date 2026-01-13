using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Cheetah.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ModuleDependencyAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            DiagnosticDescriptors.MissingModuleDependency,
            DiagnosticDescriptors.UnusedModuleDependency);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
    }

    private void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);

        if (classSymbol == null || classSymbol.IsAbstract)
            return;

        // Check if this class is a CrmModule
        if (!IsModuleClass(classSymbol))
            return;

        // Get direct project references from MSBuild
        var directProjectReferences = GetDirectProjectReferences(context.Options);

        // Get all [DependsOn] attributes from syntax
        var dependsOnAttributes = GetDependsOnAttributesFromSyntax(classDeclaration, context.SemanticModel);

        // Get all referenced modules (only from direct project references)
        var referencedModules = GetDirectlyReferencedModules(context.Compilation, directProjectReferences);

        // Check for missing dependencies
        var dependsOnTypes = new HashSet<string>(dependsOnAttributes.Select(d => d.ModuleTypeFullName));
        foreach (var referencedModule in referencedModules)
        {
            var referencedModuleFullName = referencedModule.ModuleType.ToDisplayString();
            if (!dependsOnTypes.Contains(referencedModuleFullName))
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.MissingModuleDependency,
                    classDeclaration.Identifier.GetLocation(),
                    referencedModule.AssemblyName,
                    classSymbol.Name,
                    referencedModule.ModuleType.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }

        // Check for unused dependencies
        foreach (var dependsOnAttr in dependsOnAttributes)
        {
            // Try to resolve the type from the attribute
            var dependsOnTypeSymbol = ResolveTypeFromAttribute(dependsOnAttr.AttributeSyntax, context.SemanticModel);
            if (dependsOnTypeSymbol == null)
                continue;

            var assemblyName = dependsOnTypeSymbol.ContainingAssembly.Name;

            if (!referencedModules.Any(rm => rm.AssemblyName == assemblyName))
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.UnusedModuleDependency,
                    dependsOnAttr.Location,
                    classSymbol.Name,
                    dependsOnTypeSymbol.Name,
                    assemblyName);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private HashSet<string> GetDirectProjectReferences(AnalyzerOptions options)
    {
        var result = new HashSet<string>();

        // Try to get DirectProjectReferences from MSBuild
        if (options.AnalyzerConfigOptionsProvider.GlobalOptions.TryGetValue("build_property.DirectProjectReferences", out var refs))
        {
            if (!string.IsNullOrEmpty(refs))
            {
                // Format: projectName1,projectName2,projectName3 (comma-separated)
                // MSBuild transformation already extracted filenames without extensions
                var projectNames = refs.Split(',');

                foreach (var name in projectNames)
                {
                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    var trimmedName = name.Trim();

                    // Exclude analyzers (they are added via Directory.Build.targets)
                    if (trimmedName == "Cheetah.Generators.Module" || trimmedName == "Cheetah.Analyzers")
                        continue;

                    result.Add(trimmedName);
                }
            }
        }

        return result;
    }

    private List<ReferencedModule> GetDirectlyReferencedModules(
        Compilation compilation,
        HashSet<string> directProjectReferences)
    {
        var modules = new List<ReferencedModule>();

        foreach (var reference in compilation.References)
        {
            var assembly = compilation.GetAssemblyOrModuleSymbol(reference) as IAssemblySymbol;
            if (assembly is null)
                continue;

            // Skip if not in direct project references
            if (directProjectReferences.Count > 0 && !directProjectReferences.Contains(assembly.Name))
                continue;

            var module = FindModuleInAssembly(assembly);
            if (module is not null)
            {
                modules.Add(new ReferencedModule(assembly.Name, module));
            }
        }

        return modules;
    }

    private bool IsModuleClass(INamedTypeSymbol classSymbol)
    {
        var currentType = classSymbol;
        while (currentType != null)
        {
            if (currentType.AllInterfaces.Any(i => i.ToDisplayString() == "Cheetah.Core.Modularity.ICrmModule"))
                return true;

            currentType = currentType.BaseType;
        }

        return false;
    }

    private List<DependsOnAttributeInfo> GetDependsOnAttributesFromSyntax(
        ClassDeclarationSyntax classDeclaration,
        SemanticModel semanticModel)
    {
        var result = new List<DependsOnAttributeInfo>();

        foreach (var attributeList in classDeclaration.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var attributeName = attribute.Name.ToString();

                // Check if it's DependsOn or DependsOnAttribute
                if (attributeName != "DependsOn" && attributeName != "DependsOnAttribute")
                    continue;

                // Get all typeof arguments (supports both single and multiple arguments)
                if (attribute.ArgumentList?.Arguments.Count > 0)
                {
                    foreach (var arg in attribute.ArgumentList.Arguments)
                    {
                        if (arg.Expression is TypeOfExpressionSyntax typeofExpr)
                        {
                            var typeInfo = semanticModel.GetTypeInfo(typeofExpr.Type);
                            if (typeInfo.Type is INamedTypeSymbol namedType)
                            {
                                result.Add(new DependsOnAttributeInfo(
                                    attribute.GetLocation(),
                                    attribute,
                                    namedType.ToDisplayString()));
                            }
                        }
                    }
                }
            }
        }

        return result;
    }

    private INamedTypeSymbol? ResolveTypeFromAttribute(
        AttributeSyntax attributeSyntax,
        SemanticModel semanticModel)
    {
        if (attributeSyntax.ArgumentList?.Arguments.Count > 0)
        {
            var firstArg = attributeSyntax.ArgumentList.Arguments[0];
            if (firstArg.Expression is TypeOfExpressionSyntax typeofExpr)
            {
                var typeInfo = semanticModel.GetTypeInfo(typeofExpr.Type);
                return typeInfo.Type as INamedTypeSymbol;
            }
        }

        return null;
    }

    private INamedTypeSymbol? FindModuleInAssembly(IAssemblySymbol assembly)
    {
        return FindModuleInNamespace(assembly.GlobalNamespace);
    }

    private INamedTypeSymbol? FindModuleInNamespace(INamespaceSymbol namespaceSymbol)
    {
        foreach (var type in namespaceSymbol.GetTypeMembers())
        {
            if (!type.IsAbstract &&
                type.AllInterfaces.Any(i => i.ToDisplayString() == "Cheetah.Core.Modularity.ICrmModule"))
            {
                return type;
            }
        }

        foreach (var nestedNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            var result = FindModuleInNamespace(nestedNamespace);
            if (result is not null)
                return result;
        }

        return null;
    }

    private record ReferencedModule(string AssemblyName, INamedTypeSymbol ModuleType);
    private record DependsOnAttributeInfo(Location Location, AttributeSyntax AttributeSyntax, string ModuleTypeFullName);
}
