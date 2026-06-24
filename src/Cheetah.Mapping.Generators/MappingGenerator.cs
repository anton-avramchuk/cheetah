using System;
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
    private const string GenerateMapperAttributeName = "Cheetah.Mapping.Core.GenerateMapperAttribute";
    private const string MapMemberAttributeName = "Cheetah.Mapping.Core.MapMemberAttribute";
    private const string MapNestedAttributeName = "Cheetah.Mapping.Core.MapNestedAttribute";
    private const string MapConstantAttributeName = "Cheetah.Mapping.Core.MapConstantAttribute";

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

    private static readonly DiagnosticDescriptor MissingMapperTypesDescriptor = new(
        id: "CHMAP03",
        title: "Missing mapper types",
        messageFormat: "[GenerateMapper] must specify both a source and a destination type",
        category: "Mapping",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Режим A: [MapFrom] на типе-приёмнике — генерация в неймспейс приёмника.
        var mapFromTargets = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is ClassDeclarationSyntax c && c.AttributeLists.Count > 0,
                transform: static (ctx, ct) => GetClassWithMapFromAttribute(ctx, ct))
            .Where(static t => t is not null)
            .Select(static (t, _) => t!)
            .Collect();

        // Режим B (класс-реестр): [GenerateMapper] на классе — генерация в неймспейс реестра.
        var registryTargets = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is ClassDeclarationSyntax c && c.AttributeLists.Count > 0,
                transform: static (ctx, ct) => GetRegistryClass(ctx, ct))
            .Where(static t => t is not null)
            .Select(static (t, _) => t!)
            .Collect();

        // Режим B (сборка): [assembly: GenerateMapper] — генерация в неймспейс по имени сборки.
        var assemblyProvider = context.CompilationProvider;

        var combined = mapFromTargets.Combine(registryTargets).Combine(assemblyProvider);

        context.RegisterSourceOutput(combined, static (spc, data) =>
        {
            var ((mapFromClasses, registryClasses), compilation) = data;
            ExecuteMapFrom(mapFromClasses, spc);
            ExecuteGenerateMapper(registryClasses, compilation, spc);
        });
    }

    // ---------- Режим A: [MapFrom] ----------

    private static INamedTypeSymbol? GetClassWithMapFromAttribute(GeneratorSyntaxContext context, System.Threading.CancellationToken ct)
    {
        var classSyntax = (ClassDeclarationSyntax)context.Node;
        if (context.SemanticModel.GetDeclaredSymbol(classSyntax, ct) is not INamedTypeSymbol typeSymbol)
            return null;

        var hasMapFrom = typeSymbol.GetAttributes()
            .Any(a => a.AttributeClass?.ToDisplayString() == MapFromAttributeName);

        return hasMapFrom ? typeSymbol : null;
    }

    private static void ExecuteMapFrom(ImmutableArray<INamedTypeSymbol> classes, SourceProductionContext context)
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

            var ns = destClass.ContainingNamespace.IsGlobalNamespace
                ? null
                : destClass.ContainingNamespace.ToDisplayString();

            var source = BuildMapper(sourceClass, destClass, ns, $"{destClass.Name}MapperExtensions", true,
                EmptyRenames, EmptyNested, EmptyConstants, context);
            if (source is null) continue;

            var nsForHint = ns is null ? "Global" : ns.Replace('.', '_');
            var hintName = $"{nsForHint}_{destClass.Name}_Mapper.g.cs";
            context.AddSource(hintName, SourceText.From(source, Encoding.UTF8));
        }
    }

    // ---------- Режим B: [GenerateMapper] ----------

    private static INamedTypeSymbol? GetRegistryClass(GeneratorSyntaxContext context, System.Threading.CancellationToken ct)
    {
        var classSyntax = (ClassDeclarationSyntax)context.Node;
        if (context.SemanticModel.GetDeclaredSymbol(classSyntax, ct) is not INamedTypeSymbol typeSymbol)
            return null;

        var hasGenerateMapper = typeSymbol.GetAttributes()
            .Any(a => a.AttributeClass?.ToDisplayString() == GenerateMapperAttributeName);

        return hasGenerateMapper ? typeSymbol : null;
    }

    private static void ExecuteGenerateMapper(
        ImmutableArray<INamedTypeSymbol> registries,
        Compilation compilation,
        SourceProductionContext context)
    {
        // Пары source→dest вместе с целевым неймспейсом, переименованиями, вложенными членами и константами.
        var specs = new List<(INamedTypeSymbol Source, INamedTypeSymbol Dest, string? Namespace, bool Projection, Dictionary<string, string> Renames, HashSet<string> Nested, Dictionary<string, string> Constants, Location? Location)>();

        // Реестры-классы: неймспейс = неймспейс реестра.
        foreach (var registry in registries.Distinct<INamedTypeSymbol>(SymbolEqualityComparer.Default))
        {
            var ns = registry.ContainingNamespace.IsGlobalNamespace
                ? null
                : registry.ContainingNamespace.ToDisplayString();

            ProcessContainer(registry.GetAttributes(), ns, registry.Locations.FirstOrDefault(), specs, context);
        }

        // Атрибуты уровня сборки: неймспейс = имя сборки.
        ProcessContainer(compilation.Assembly.GetAttributes(), compilation.AssemblyName, null, specs, context);

        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var spec in specs)
        {
            var className = $"{spec.Source.Name}To{spec.Dest.Name}Mapper";
            var dedupKey = $"{spec.Namespace}::{spec.Source.ToDisplayString()}::{spec.Dest.ToDisplayString()}";
            if (!seen.Add(dedupKey)) continue;

            var source = BuildMapper(spec.Source, spec.Dest, spec.Namespace, className, spec.Projection, spec.Renames, spec.Nested, spec.Constants, context);
            if (source is null) continue;

            var nsForHint = spec.Namespace is null ? "Global" : spec.Namespace.Replace('.', '_');
            var hintName = $"{nsForHint}_{spec.Source.Name}_To_{spec.Dest.Name}_Mapper.g.cs";
            context.AddSource(hintName, SourceText.From(source, Encoding.UTF8));
        }
    }

    private static void ProcessContainer(
        ImmutableArray<AttributeData> attributes,
        string? ns,
        Location? location,
        List<(INamedTypeSymbol, INamedTypeSymbol, string?, bool, Dictionary<string, string>, HashSet<string>, Dictionary<string, string>, Location?)> specs,
        SourceProductionContext context)
    {
        // Переименования членов (dest-член ← source-член), сгруппированные по типу-приёмнику.
        var memberRenames = new List<(INamedTypeSymbol Dest, string DestMember, string SourceMember)>();
        foreach (var attr in attributes.Where(a => a.AttributeClass?.ToDisplayString() == MapMemberAttributeName))
        {
            if (attr.ConstructorArguments.Length >= 3 &&
                attr.ConstructorArguments[0].Value is INamedTypeSymbol destType &&
                attr.ConstructorArguments[1].Value is string destMember &&
                attr.ConstructorArguments[2].Value is string sourceMember)
            {
                memberRenames.Add((destType, destMember, sourceMember));
            }
        }

        // Вложенные члены (собираются как отдельный объект из того же источника), по типу-приёмнику.
        var nestedMembers = new List<(INamedTypeSymbol Dest, string DestMember)>();
        foreach (var attr in attributes.Where(a => a.AttributeClass?.ToDisplayString() == MapNestedAttributeName))
        {
            if (attr.ConstructorArguments.Length >= 2 &&
                attr.ConstructorArguments[0].Value is INamedTypeSymbol destType &&
                attr.ConstructorArguments[1].Value is string destMember)
            {
                nestedMembers.Add((destType, destMember));
            }
        }

        // Константные члены (значение независимо от источника), по типу-приёмнику.
        var constantMembers = new List<(INamedTypeSymbol Dest, string DestMember, string Literal)>();
        foreach (var attr in attributes.Where(a => a.AttributeClass?.ToDisplayString() == MapConstantAttributeName))
        {
            if (attr.ConstructorArguments.Length >= 3 &&
                attr.ConstructorArguments[0].Value is INamedTypeSymbol destType &&
                attr.ConstructorArguments[1].Value is string destMember)
            {
                constantMembers.Add((destType, destMember, RenderConstant(attr.ConstructorArguments[2])));
            }
        }

        foreach (var attr in attributes.Where(a => a.AttributeClass?.ToDisplayString() == GenerateMapperAttributeName))
        {
            if (attr.ConstructorArguments.Length < 2 ||
                attr.ConstructorArguments[0].Value is not INamedTypeSymbol source ||
                attr.ConstructorArguments[1].Value is not INamedTypeSymbol dest)
            {
                context.ReportDiagnostic(Diagnostic.Create(MissingMapperTypesDescriptor, location));
                continue;
            }

            var projection = true;
            var projectionArg = attr.NamedArguments
                .FirstOrDefault(a => a.Key == "GenerateProjection");
            if (projectionArg.Key == "GenerateProjection" && projectionArg.Value.Value is bool b)
                projection = b;

            var renames = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var o in memberRenames.Where(o => SymbolEqualityComparer.Default.Equals(o.Dest, dest)))
                renames[o.DestMember] = o.SourceMember;

            var nested = new HashSet<string>(StringComparer.Ordinal);
            foreach (var o in nestedMembers.Where(o => SymbolEqualityComparer.Default.Equals(o.Dest, dest)))
                nested.Add(o.DestMember);

            var constants = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var o in constantMembers.Where(o => SymbolEqualityComparer.Default.Equals(o.Dest, dest)))
                constants[o.DestMember] = o.Literal;

            specs.Add((source, dest, ns, projection, renames, nested, constants, location));
        }
    }

    // ---------- Общее построение маппера (конструктор-aware) ----------

    private static string? BuildMapper(
        INamedTypeSymbol sourceClass,
        INamedTypeSymbol destClass,
        string? ns,
        string className,
        bool generateProjection,
        Dictionary<string, string> renames,
        HashSet<string> nested,
        Dictionary<string, string> constants,
        SourceProductionContext context)
    {
        if (!TryBuildConstruction(sourceClass, destClass, renames, nested, constants, context, out var ctorArgs, out var initBindings))
            return null;

        var destFqn = destClass.ToDisplayString();
        var sourceFqn = sourceClass.ToDisplayString();

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

        sb.Append("public static class ").Append(className).AppendLine();
        sb.AppendLine("{");

        // Scalar map
        sb.Append("    public static ").Append(destFqn).Append("? MapTo").Append(destClass.Name)
            .Append("(this ").Append(sourceFqn).AppendLine("? source)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (source is null) return null;");
        AppendConstruction(sb, "        ", "return ", destFqn, ctorArgs, initBindings, "source.");
        sb.AppendLine("    }");

        // IQueryable projection — только если запрошена (бесполезна для Request → Command/Query).
        if (generateProjection)
        {
            sb.AppendLine();
            sb.Append("    public static System.Linq.IQueryable<").Append(destFqn).Append("> ProjectTo").Append(destClass.Name)
                .Append("(this System.Linq.IQueryable<").Append(sourceFqn).AppendLine("> query)");
            sb.AppendLine("    {");
            AppendProjection(sb, "        ", destFqn, ctorArgs, initBindings);
            sb.AppendLine("    }");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    // Свойства типа вместе с унаследованными (Domain-сущности наследуют Id от Entity&lt;T&gt;,
    // grid-запросы — пагинацию от базового запроса). Более производные перекрывают базовые по имени.
    private static IEnumerable<IPropertySymbol> GetAllProperties(INamedTypeSymbol type)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (INamedTypeSymbol? t = type; t is not null && t.SpecialType != SpecialType.System_Object; t = t.BaseType)
        {
            foreach (var p in t.GetMembers().OfType<IPropertySymbol>())
            {
                if (p.IsStatic || p.Parameters.Length > 0) // пропускаем индексаторы
                    continue;
                if (seen.Add(p.Name))
                    yield return p;
            }
        }
    }

    private static bool TryResolveSource(
        Dictionary<string, IPropertySymbol> exact,
        Dictionary<string, IPropertySymbol> ci,
        string name,
        out string resolvedName)
    {
        if (exact.TryGetValue(name, out var p))
        {
            resolvedName = p.Name;
            return true;
        }

        if (ci.TryGetValue(name, out p))
        {
            resolvedName = p.Name;
            return true;
        }

        resolvedName = name;
        return false;
    }

    // Привязки значений для конструктора/инициализатора. Каждое значение — функция от accessor
    // ("source." / "x."), чтобы вложенные выражения подставляли правильный префикс.
    private static bool TryBuildConstruction(
        INamedTypeSymbol sourceClass,
        INamedTypeSymbol destClass,
        Dictionary<string, string> renames,
        HashSet<string> nested,
        Dictionary<string, string> constants,
        SourceProductionContext context,
        out List<Func<string, string>> ctorArgs,
        out List<(string DestName, Func<string, string> Value)> initBindings)
    {
        ctorArgs = new List<Func<string, string>>();
        initBindings = new List<(string, Func<string, string>)>();

        var sourceProps = new Dictionary<string, IPropertySymbol>(StringComparer.Ordinal);
        var sourcePropsCi = new Dictionary<string, IPropertySymbol>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in GetAllProperties(sourceClass).Where(p => p.GetMethod is not null))
        {
            if (!sourceProps.ContainsKey(p.Name))
                sourceProps[p.Name] = p;
            if (!sourcePropsCi.ContainsKey(p.Name))
                sourcePropsCi[p.Name] = p;
        }

        // Публичный конструктор с максимальным числом параметров
        // (для record это первичный конструктор; для классов с {get;set;} — пустой).
        var ctor = destClass.InstanceConstructors
            .Where(c => c.DeclaredAccessibility == Accessibility.Public && !c.IsStatic)
            .OrderByDescending(c => c.Parameters.Length)
            .FirstOrDefault();

        var ctorParamNames = ctor is null
            ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(ctor.Parameters.Select(p => p.Name), StringComparer.OrdinalIgnoreCase);

        var hadError = false;

        if (ctor is not null)
        {
            foreach (var p in ctor.Parameters)
            {
                if (TryResolveValue(sourceClass, sourceProps, sourcePropsCi, p.Name, p.Type, null,
                        renames, nested, constants, context, destClass, out var renderer))
                    ctorArgs.Add(renderer!);
                else
                    hadError = true;
            }
        }

        // Свойства с сеттером, не покрытые конструктором, — через инициализатор объекта.
        var seenDestProps = new HashSet<string>(StringComparer.Ordinal);
        foreach (var destProp in GetAllProperties(destClass).Where(p => p.SetMethod is not null))
        {
            if (!seenDestProps.Add(destProp.Name))
                continue;
            if (ctorParamNames.Contains(destProp.Name))
                continue;
            if (destProp.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == MapIgnoreAttributeName))
                continue;

            if (TryResolveValue(sourceClass, sourceProps, sourcePropsCi, destProp.Name, destProp.Type, destProp,
                    renames, nested, constants, context, destClass, out var renderer))
                initBindings.Add((destProp.Name, renderer!));
            else
                hadError = true;
        }

        return !hadError;
    }

    // Определяет, как получить значение для члена приёмника: вложенный объект, переименование,
    // [MapProperty] или совпадение по имени. Возвращает функцию от accessor.
    private static bool TryResolveValue(
        INamedTypeSymbol sourceClass,
        Dictionary<string, IPropertySymbol> sourceProps,
        Dictionary<string, IPropertySymbol> sourcePropsCi,
        string destMemberName,
        ITypeSymbol destMemberType,
        IPropertySymbol? destProp,
        Dictionary<string, string> renames,
        HashSet<string> nested,
        Dictionary<string, string> constants,
        SourceProductionContext context,
        INamedTypeSymbol destClass,
        out Func<string, string>? renderer)
    {
        renderer = null;

        // 0) Константа: значение не зависит от источника.
        if (constants.TryGetValue(destMemberName, out var literal))
        {
            renderer = _ => literal;
            return true;
        }

        // 1) Вложенный объект: тот же источник → тип члена (по обычным правилам).
        if (nested.Contains(destMemberName))
        {
            if (destMemberType is not INamedTypeSymbol nestedDest ||
                !TryBuildConstruction(sourceClass, nestedDest, EmptyRenames, EmptyNested, EmptyConstants, context,
                    out var nestedCtorArgs, out var nestedInit))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    UnmappedPropertyDescriptor,
                    destClass.Locations.FirstOrDefault(),
                    destMemberName, destClass.Name, sourceClass.Name));
                return false;
            }

            var nestedFqn = destMemberType.ToDisplayString();
            renderer = acc => RenderInline(nestedFqn, nestedCtorArgs, nestedInit, acc);
            return true;
        }

        // 2) [MapMember] из реестра имеет приоритет; иначе — [MapProperty] на самом свойстве.
        string lookup;
        if (renames.TryGetValue(destMemberName, out var ov))
        {
            lookup = ov;
        }
        else
        {
            lookup = destMemberName;
            var mapPropAttr = destProp?.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == MapPropertyAttributeName);
            if (mapPropAttr is not null && mapPropAttr.ConstructorArguments.Length > 0 &&
                mapPropAttr.ConstructorArguments[0].Value is string customName)
            {
                lookup = customName;
            }
        }

        if (!TryResolveSource(sourceProps, sourcePropsCi, lookup, out var srcName))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                UnmappedPropertyDescriptor,
                (destProp?.Locations ?? destClass.Locations).FirstOrDefault(),
                destMemberName, destClass.Name, sourceClass.Name));
            return false;
        }

        renderer = acc => acc + srcName;
        return true;
    }

    private static readonly Dictionary<string, string> EmptyRenames = new(StringComparer.Ordinal);
    private static readonly HashSet<string> EmptyNested = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, string> EmptyConstants = new(StringComparer.Ordinal);

    // C#-литерал из значения атрибута: "Bearer", 5, true, (Ns.Enum)value, null.
    private static string RenderConstant(TypedConstant c)
    {
        if (c.IsNull)
            return "null";

        var v = c.Value;
        if (c.Kind == TypedConstantKind.Enum && c.Type is not null && v is not null)
            return "(" + c.Type.ToDisplayString() + ")" + System.Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture);

        switch (v)
        {
            case null:
                return "default";
            case string s:
                return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n") + "\"";
            case bool b:
                return b ? "true" : "false";
            case char ch:
                return "'" + (ch == '\'' ? "\\'" : ch == '\\' ? "\\\\" : ch.ToString()) + "'";
            case float f:
                return f.ToString(System.Globalization.CultureInfo.InvariantCulture) + "f";
            case double d:
                return d.ToString(System.Globalization.CultureInfo.InvariantCulture) + "d";
            case decimal m:
                return m.ToString(System.Globalization.CultureInfo.InvariantCulture) + "m";
            default:
                return System.Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture) ?? "default";
        }
    }

    // Однострочное выражение new T(...) { ... } для подстановки как аргумент/значение.
    private static string RenderInline(
        string destFqn,
        List<Func<string, string>> ctorArgs,
        List<(string DestName, Func<string, string> Value)> initBindings,
        string accessor)
    {
        var sb = new StringBuilder();
        sb.Append("new ").Append(destFqn);
        if (ctorArgs.Count > 0)
            sb.Append('(').Append(string.Join(", ", ctorArgs.Select(a => a(accessor)))).Append(')');
        else if (initBindings.Count == 0)
            sb.Append("()");

        if (initBindings.Count > 0)
        {
            sb.Append(" { ");
            sb.Append(string.Join(", ", initBindings.Select(b => b.DestName + " = " + b.Value(accessor))));
            sb.Append(" }");
        }

        return sb.ToString();
    }

    private static void AppendConstruction(
        StringBuilder sb,
        string indent,
        string prefix,
        string destFqn,
        List<Func<string, string>> ctorArgs,
        List<(string DestName, Func<string, string> Value)> initBindings,
        string accessor)
    {
        sb.Append(indent).Append(prefix).Append("new ").Append(destFqn);
        if (ctorArgs.Count > 0)
            sb.Append('(').Append(string.Join(", ", ctorArgs.Select(a => a(accessor)))).Append(')');

        if (initBindings.Count == 0)
        {
            if (ctorArgs.Count == 0)
                sb.Append("()");
            sb.AppendLine(";");
            return;
        }

        sb.AppendLine();
        sb.Append(indent).AppendLine("{");
        foreach (var b in initBindings)
        {
            sb.Append(indent).Append("    ").Append(b.DestName).Append(" = ").Append(b.Value(accessor)).AppendLine(",");
        }
        sb.Append(indent).AppendLine("};");
    }

    private static void AppendProjection(
        StringBuilder sb,
        string indent,
        string destFqn,
        List<Func<string, string>> ctorArgs,
        List<(string DestName, Func<string, string> Value)> initBindings)
    {
        sb.Append(indent).Append("return query.Select(x => new ").Append(destFqn);
        if (ctorArgs.Count > 0)
            sb.Append('(').Append(string.Join(", ", ctorArgs.Select(a => a("x.")))).Append(')');

        if (initBindings.Count == 0)
        {
            if (ctorArgs.Count == 0)
                sb.Append("()");
            sb.AppendLine(");");
            return;
        }

        sb.AppendLine();
        sb.Append(indent).AppendLine("{");
        foreach (var b in initBindings)
        {
            sb.Append(indent).Append("    ").Append(b.DestName).Append(" = ").Append(b.Value("x.")).AppendLine(",");
        }
        sb.Append(indent).AppendLine("});");
    }
}
