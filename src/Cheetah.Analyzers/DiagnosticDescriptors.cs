using Microsoft.CodeAnalysis;

namespace Cheetah.Analyzers;

internal static class DiagnosticDescriptors
{
    private const string Category = "Cheetah.Modularity";

    public static readonly DiagnosticDescriptor MissingModuleDependency = new(
        id: "CHT001",
        title: "Missing module dependency attribute",
        messageFormat: "Project references '{0}' but module '{1}' is missing [DependsOn(typeof({2}))]",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "When a project references another project that contains a CrmModule, the module class must have a [DependsOn] attribute for the referenced module.",
        customTags: "CompilationEnd");

    public static readonly DiagnosticDescriptor UnusedModuleDependency = new(
        id: "CHT002",
        title: "Unused module dependency attribute",
        messageFormat: "Module '{0}' has [DependsOn(typeof({1}))] but project doesn't reference '{2}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Module has a [DependsOn] attribute for a module that is not referenced by the project.",
        customTags: "CompilationEnd");
}
