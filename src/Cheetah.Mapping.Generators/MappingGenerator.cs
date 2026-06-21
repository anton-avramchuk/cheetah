using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Cheetah.Mapping.Generators;

[Generator]
public class MappingGenerator : IIncrementalGenerator
{
    private const string MapFromAttributeName = "Cheetah.Mapping.Core.MapFromAttribute";
    private const string MapIgnoreAttributeName = "Cheetah.Mapping.Core.MapIgnoreAttribute";
    private const string MapPropertyAttributeName = "Cheetah.Mapping.Core.MapPropertyAttribute";

    private static readonly DiagnosticDescriptor UnmappedPropertyDescriptor = new(
        id: "CHMAP01",
        title: "Unmapped Property",
        messageFormat: "Property '{0}' in destination '{1}' is not mapped to any source property in '{2}'. Use [MapIgnore] or [MapProperty] to resolve.",
        category: "Mapping",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor MissingSourceTypeDescriptor = new(
        id: "CHMAP02",
        title: "Missing source type",
        messageFormat: "[MapFrom] on '{0}' does not specify a valid source type",
        category: "Mapping",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var targets = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is ClassDeclarationSyntax c && c.AttributeLists.Count > 0,
                transform: static (ctx, ct) => GetClassWithMapFromAttribute(ctx, ct))
            .Where(static t => t is not null)
            .Select(static (t, _) => t!)
            .Collect();

        context.RegisterSourceOutput(targets, static (spc, classes) => Execute(classes, spc));
    }

    private static INamedTypeSymbol? GetClassWithMapFromAttribute(GeneratorSyntaxContext context, System.Threading.CancellationToken ct)
    {
        var classSyntax = (ClassDeclarationSyntax)context.Node;
        if (context.SemanticModel.GetDeclaredSymbol(classSyntax, ct) is not INamedTypeSymbol typeSymbol)
            return null;

        var hasMapFrom = typeSymbol.GetAttributes()
            .Any(a => a.AttributeClass?.ToDisplayString() == MapFromAttributeName);

        return hasMapFrom ? typeSymbol : null;
    }

    private static void Execute(ImmutableArray<INamedTypeSymbol> classes, SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty) return;

        foreach (var destClass in classes.Distinct<INamedTypeSymbol>(SymbolEqualityComparer.Default))
        {
            var mapFromAttr = destClass.GetAttributes()
                .First(a => a.AttributeClass?.ToDisplayString() == MapFromAttributeName);

            if (mapFromAttr.ConstructorArguments.FirstOrDefault().Value is not INamedTypeSymbol sourceClass)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    MissingSourceTypeDescriptor,
                    destClass.Locations.FirstOrDefault(),
                    destClass.Name));
                continue;
            }

            var source = GenerateForClass(sourceClass, destClass, context);
            if (source is null) continue;

            var nsForHint = destClass.ContainingNamespace.IsGlobalNamespace
                ? "Global"
                : destClass.ContainingNamespace.ToDisplayString().Replace('.', '_');
            var hintName = $"{nsForHint}_{destClass.Name}_Mapper.g.cs";
            context.AddSource(hintName, SourceText.From(source, Encoding.UTF8));
        }
    }

    private static string? GenerateForClass(
        INamedTypeSymbol sourceClass,
        INamedTypeSymbol destClass,
        SourceProductionContext context)
    {
        var destProps = destClass.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.SetMethod is not null) // включает set и init
            .ToList();

        var sourceProps = sourceClass.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.GetMethod is not null)
            .ToDictionary(p => p.Name, p => p);

        var bindings = new List<(string DestName, string SourceName)>();
        var hadError = false;

        foreach (var destProp in destProps)
        {
            if (destProp.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == MapIgnoreAttributeName))
                continue;

            var mapPropAttr = destProp.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == MapPropertyAttributeName);

            var sourcePropName = destProp.Name;
            if (mapPropAttr is not null && mapPropAttr.ConstructorArguments.Length > 0 &&
                mapPropAttr.ConstructorArguments[0].Value is string customName)
            {
                sourcePropName = customName;
            }

            if (!sourceProps.TryGetValue(sourcePropName, out var _))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    UnmappedPropertyDescriptor,
                    destProp.Locations.FirstOrDefault(),
                    destProp.Name, destClass.Name, sourceClass.Name));
                hadError = true;
                continue;
            }

            bindings.Add((destProp.Name, sourcePropName));
        }

        if (hadError) return null;

        var destFqn = destClass.ToDisplayString();
        var sourceFqn = sourceClass.ToDisplayString();
        var ns = destClass.ContainingNamespace.IsGlobalNamespace
            ? null
            : destClass.ContainingNamespace.ToDisplayString();

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine();

        if (ns is not null)
        {
            sb.Append("namespace ").Append(ns).AppendLine(";");
            sb.AppendLine();
        }

        sb.Append("public static class ").Append(destClass.Name).AppendLine("MapperExtensions");
        sb.AppendLine("{");

        // Scalar map
        sb.Append("    public static ").Append(destFqn).Append("? MapTo").Append(destClass.Name)
            .Append("(this ").Append(sourceFqn).AppendLine("? source)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (source is null) return null;");
        sb.Append("        return new ").Append(destFqn).AppendLine();
        sb.AppendLine("        {");
        foreach (var b in bindings)
        {
            sb.Append("            ").Append(b.DestName).Append(" = source.").Append(b.SourceName).AppendLine(",");
        }
        sb.AppendLine("        };");
        sb.AppendLine("    }");
        sb.AppendLine();

        // IQueryable projection
        sb.Append("    public static System.Linq.IQueryable<").Append(destFqn).Append("> ProjectTo").Append(destClass.Name)
            .Append("(this System.Linq.IQueryable<").Append(sourceFqn).AppendLine("> query)");
        sb.AppendLine("    {");
        sb.Append("        return query.Select(x => new ").Append(destFqn).AppendLine();
        sb.AppendLine("        {");
        foreach (var b in bindings)
        {
            sb.Append("            ").Append(b.DestName).Append(" = x.").Append(b.SourceName).AppendLine(",");
        }
        sb.AppendLine("        });");
        sb.AppendLine("    }");

        sb.AppendLine("}");
        return sb.ToString();
    }
}
