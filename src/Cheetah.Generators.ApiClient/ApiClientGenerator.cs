using System.Collections.Immutable;
using System.Text;
using Cheetah.Contracts.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Cheetah.Generators.ApiClient;

[Generator]
public class ApiClientGenerator : IIncrementalGenerator
{
    private const string GenerateApiClientAttributeName = "Cheetah.Contracts.Attributes.GenerateApiClientAttribute";
    private const string ApiRouteAttributeName = "Cheetah.Contracts.Attributes.ApiRouteAttribute";
    private const string FromRouteAttributeName = "Cheetah.Contracts.Attributes.FromRouteAttribute";
    private const string GridRequestTypeName = "Cheetah.Contracts.Requests.GridRequest";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var moduleProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax cds &&
                    cds.Modifiers.Any(SyntaxKind.PartialKeyword),
                transform: static (ctx, _) => GetModuleInfo(ctx))
            .Where(static m => m is not null);

        var apiRouteProvider = context.CompilationProvider
            .Select(static (compilation, ct) => GetApiRouteTypes(compilation, ct));

        var combined = moduleProvider.Collect().Combine(apiRouteProvider);

        context.RegisterSourceOutput(combined, static (ctx, source) =>
        {
            var (modules, apiRoutes) = source;

            if (modules.IsDefaultOrEmpty)
                return;

            var moduleInfo = modules[0]!;

            if (apiRoutes.IsDefaultOrEmpty)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("APIGEN001", "No ApiRoute types found",
                        "No types with [ApiRoute] attribute were found in referenced assemblies",
                        "ApiClientGenerator", DiagnosticSeverity.Warning, true),
                    Location.None));
                return;
            }

            Generate(ctx, moduleInfo, apiRoutes);
        });
    }

    private static ModuleInfo? GetModuleInfo(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);

        if (symbol is null)
            return null;

        var attr = symbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == GenerateApiClientAttributeName);

        if (attr is null)
            return null;

        var serviceName = attr.ConstructorArguments[0].Value as string;
        if (string.IsNullOrEmpty(serviceName))
            return null;

        return new ModuleInfo(
            symbol.Name,
            symbol.ContainingNamespace.ToDisplayString(),
            serviceName!);
    }

    private static ImmutableArray<ApiRouteInfo> GetApiRouteTypes(Compilation compilation, System.Threading.CancellationToken ct)
    {
        var results = ImmutableArray.CreateBuilder<ApiRouteInfo>();

        SearchAssembly(compilation.Assembly, compilation, results, ct);

        foreach (var reference in compilation.References)
        {
            ct.ThrowIfCancellationRequested();
            var refSymbol = compilation.GetAssemblyOrModuleSymbol(reference);
            if (refSymbol is IAssemblySymbol assemblySymbol)
            {
                SearchAssembly(assemblySymbol, compilation, results, ct);
            }
        }

        return results.ToImmutable();
    }

    private static void SearchAssembly(IAssemblySymbol assembly, Compilation compilation,
        ImmutableArray<ApiRouteInfo>.Builder results, System.Threading.CancellationToken ct)
    {
        SearchNamespace(assembly.GlobalNamespace, compilation, results, ct);
    }

    private static void SearchNamespace(INamespaceSymbol ns, Compilation compilation,
        ImmutableArray<ApiRouteInfo>.Builder results, System.Threading.CancellationToken ct)
    {
        foreach (var type in ns.GetTypeMembers())
        {
            ct.ThrowIfCancellationRequested();
            TryAddApiRouteType(type, compilation, results);
        }

        foreach (var nestedNs in ns.GetNamespaceMembers())
        {
            SearchNamespace(nestedNs, compilation, results, ct);
        }
    }

    private static void TryAddApiRouteType(INamedTypeSymbol type, Compilation compilation,
        ImmutableArray<ApiRouteInfo>.Builder results)
    {
        var attr = type.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == ApiRouteAttributeName);

        if (attr is null)
            return;

        var route = attr.ConstructorArguments[0].Value as string ?? "";
        route = route.TrimStart('/');
        if (route.StartsWith("api/", System.StringComparison.OrdinalIgnoreCase))
            route = route.Substring(4);
        var method = (ApiMethod)(int)(attr.ConstructorArguments[1].Value ?? 0);

        string? responseTypeName = null;
        string? methodName = null;
        string? serviceName = null;

        foreach (var named in attr.NamedArguments)
        {
            if (named.Key == "ResponseType" && named.Value.Value is INamedTypeSymbol responseType)
            {
                responseTypeName = responseType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            }
            else if (named.Key == "MethodName" && named.Value.Value is string mn)
            {
                methodName = mn;
            }
            else if (named.Key == "ServiceName" && named.Value.Value is string sn)
            {
                serviceName = sn;
            }
        }

        // Find [FromRoute] properties/parameters
        var routeParams = new List<RouteParamInfo>();
        foreach (var member in type.GetMembers())
        {
            if (member is IPropertySymbol prop)
            {
                var fromRoute = prop.GetAttributes()
                    .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == FromRouteAttributeName);
                if (fromRoute is not null)
                {
                    routeParams.Add(new RouteParamInfo(
                        prop.Name,
                        prop.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
                }
            }
        }

        // Also check constructor parameters (for records)
        foreach (var ctor in type.Constructors)
        {
            foreach (var param in ctor.Parameters)
            {
                var fromRoute = param.GetAttributes()
                    .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == FromRouteAttributeName);
                if (fromRoute is not null)
                {
                    if (!routeParams.Any(rp => string.Equals(rp.Name, param.Name, System.StringComparison.OrdinalIgnoreCase)))
                    {
                        routeParams.Add(new RouteParamInfo(
                            param.Name,
                            param.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
                    }
                }
            }
        }

        var isGridRequest = InheritsFrom(type, GridRequestTypeName);

        results.Add(new ApiRouteInfo(
            type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            type.Name,
            route,
            method,
            responseTypeName,
            methodName,
            serviceName,
            routeParams.ToArray(),
            isGridRequest));
    }

    private static bool InheritsFrom(INamedTypeSymbol type, string baseTypeName)
    {
        var current = type.BaseType;
        while (current is not null)
        {
            if (current.ToDisplayString() == baseTypeName)
                return true;
            current = current.BaseType;
        }
        return false;
    }

    private static void Generate(SourceProductionContext ctx, ModuleInfo module, ImmutableArray<ApiRouteInfo> routes)
    {
        // Group routes by ServiceName (fallback to module's default name)
        var groups = routes
            .GroupBy(r => r.ServiceName ?? module.ServiceName)
            .ToList();

        var serviceRegistrations = new List<ServiceRegistration>();
        var hasCreateEntityResponse = false;

        foreach (var group in groups)
        {
            var serviceName = group.Key;
            var interfaceName = $"I{serviceName}Service";
            var className = $"{serviceName}Service";

            // Build method infos
            var methods = new List<MethodInfo>();
            foreach (var route in group)
            {
                var info = BuildMethodInfo(route);
                if (info is not null)
                    methods.Add(info);
            }

            // Check for name collisions within this group
            var nameGroups = methods.GroupBy(m => m.Name).Where(g => g.Count() > 1).ToList();
            if (nameGroups.Any())
            {
                foreach (var ng in nameGroups)
                {
                    ctx.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor("APIGEN002", "Method name collision",
                            $"Multiple [ApiRoute] types in service '{serviceName}' generate the same method name '{ng.Key}'. Use MethodName or ServiceName to disambiguate.",
                            "ApiClientGenerator", DiagnosticSeverity.Error, true),
                        Location.None));
                }
                return;
            }

            // Generate interface
            GenerateInterface(ctx, module, interfaceName, methods);

            // Generate implementation (only first group emits CreateEntityResponse)
            var needsCreateEntityResponse = methods.Any(m => m.Route.Method == ApiMethod.Create);
            GenerateService(ctx, module, interfaceName, className, methods,
                emitCreateEntityResponse: needsCreateEntityResponse && !hasCreateEntityResponse);
            if (needsCreateEntityResponse)
                hasCreateEntityResponse = true;

            serviceRegistrations.Add(new ServiceRegistration(interfaceName, className));
        }

        // Generate single module partial class with all registrations
        GenerateModulePartial(ctx, module, serviceRegistrations);
    }

    private static MethodInfo? BuildMethodInfo(ApiRouteInfo route)
    {
        var name = route.MethodName;

        if (string.IsNullOrEmpty(name))
        {
            name = route.Method switch
            {
                ApiMethod.Create => "CreateAsync",
                ApiMethod.GetOrNotFound => "GetByIdAsync",
                ApiMethod.GetGrid => "GetAllAsync",
                ApiMethod.GetCollection => "GetAllAsync",
                ApiMethod.Update => "UpdateAsync",
                ApiMethod.UpdateWithResult => "UpdateAsync",
                ApiMethod.Delete => "DeleteAsync",
                ApiMethod.Get => "GetAsync",
                ApiMethod.Post => "PostAsync",
                ApiMethod.PostWithResult => "PostAsync",
                ApiMethod.Patch => "PatchAsync",
                _ => null
            };
        }

        if (name is null)
            return null;

        return new MethodInfo(name, route);
    }

    private static void GenerateInterface(SourceProductionContext ctx, ModuleInfo module,
        string interfaceName, List<MethodInfo> methods)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine($"namespace {module.Namespace};");
        sb.AppendLine();
        sb.AppendLine($"public interface {interfaceName}");
        sb.AppendLine("{");

        foreach (var method in methods)
        {
            sb.AppendLine($"    {GetMethodSignature(method)};");
        }

        sb.AppendLine("}");

        ctx.AddSource($"{interfaceName}.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static void GenerateService(SourceProductionContext ctx, ModuleInfo module,
        string interfaceName, string className, List<MethodInfo> methods, bool emitCreateEntityResponse)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using System.Net;");
        sb.AppendLine("using System.Net.Http.Json;");
        sb.AppendLine("using Cheetah.Contracts.Requests;");
        sb.AppendLine("using Cheetah.Contracts.Responses;");
        sb.AppendLine();
        sb.AppendLine($"namespace {module.Namespace};");

        if (emitCreateEntityResponse)
        {
            sb.AppendLine();
            sb.AppendLine("internal record CreateEntityResponse(global::System.Guid Id);");
        }

        sb.AppendLine();
        sb.AppendLine($"public class {className}({GetHttpClientParamType()} httpClient) : {interfaceName}");
        sb.AppendLine("{");

        foreach (var method in methods)
        {
            sb.AppendLine();
            GenerateMethodBody(sb, method);
        }

        sb.AppendLine("}");

        ctx.AddSource($"{className}.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static void GenerateModulePartial(SourceProductionContext ctx, ModuleInfo module,
        List<ServiceRegistration> registrations)
    {
        var optionsClassName = module.ClassName.EndsWith("Module")
            ? module.ClassName.Substring(0, module.ClassName.Length - "Module".Length) + "Options"
            : module.ClassName + "Options";

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine("using Microsoft.Extensions.Options;");
        sb.AppendLine("using Cheetah.Core.Modularity;");
        sb.AppendLine();
        sb.AppendLine($"namespace {module.Namespace};");
        sb.AppendLine();
        sb.AppendLine($"public class {optionsClassName}");
        sb.AppendLine("{");
        sb.AppendLine("    public string BaseUrl { get; set; } = \"http://localhost:5000\";");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine($"public partial class {module.ClassName}");
        sb.AppendLine("{");
        sb.AppendLine("    public override void ConfigureServices(ServiceConfigurationContext context)");
        sb.AppendLine("    {");
        sb.AppendLine("        RegisterServices(context.Services);");
        sb.AppendLine();
        sb.AppendLine($"        context.Services.AddOptions<{optionsClassName}>()");
        sb.AppendLine($"            .BindConfiguration(\"{module.ServiceName}\");");

        foreach (var reg in registrations)
        {
            sb.AppendLine();
            sb.AppendLine($"        context.Services.AddHttpClient<{reg.InterfaceName}, {reg.ClassName}>((sp, client) =>");
            sb.AppendLine("        {");
            sb.AppendLine($"            var options = sp.GetRequiredService<IOptions<{optionsClassName}>>().Value;");
            sb.AppendLine("            client.BaseAddress = new global::System.Uri(options.BaseUrl);");
            sb.AppendLine("        });");
        }

        sb.AppendLine();
        sb.AppendLine("        ConfigureServicesCustom(context);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    partial void ConfigureServicesCustom(ServiceConfigurationContext context);");
        sb.AppendLine("}");

        ctx.AddSource($"{module.ClassName}.ApiClient.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static string GetHttpClientParamType()
    {
        return "global::System.Net.Http.HttpClient";
    }

    private static string GetMethodSignature(MethodInfo method)
    {
        var route = method.Route;
        var returnType = GetReturnType(route);
        var parameters = GetParameters(route);

        return $"{returnType} {method.Name}({parameters})";
    }

    private static string GetReturnType(ApiRouteInfo route)
    {
        return route.Method switch
        {
            ApiMethod.Create => "global::System.Threading.Tasks.ValueTask<global::System.Guid>",
            ApiMethod.GetOrNotFound => $"global::System.Threading.Tasks.ValueTask<{route.ResponseTypeName}?>",
            ApiMethod.GetGrid => $"global::System.Threading.Tasks.ValueTask<global::Cheetah.Contracts.Responses.GridResult<{route.ResponseTypeName}>>",
            ApiMethod.GetCollection => $"global::System.Threading.Tasks.ValueTask<global::System.Collections.Generic.IReadOnlyList<{route.ResponseTypeName}>>",
            ApiMethod.Get => $"global::System.Threading.Tasks.ValueTask<{route.ResponseTypeName}>",
            ApiMethod.PostWithResult => $"global::System.Threading.Tasks.ValueTask<{route.ResponseTypeName}>",
            ApiMethod.UpdateWithResult => $"global::System.Threading.Tasks.ValueTask<{route.ResponseTypeName}>",
            ApiMethod.Update => "global::System.Threading.Tasks.ValueTask",
            ApiMethod.Delete => "global::System.Threading.Tasks.ValueTask",
            ApiMethod.Post => "global::System.Threading.Tasks.ValueTask",
            ApiMethod.Patch => "global::System.Threading.Tasks.ValueTask",
            _ => "global::System.Threading.Tasks.ValueTask"
        };
    }

    private static string GetParameters(ApiRouteInfo route)
    {
        var parts = new List<string>();

        switch (route.Method)
        {
            case ApiMethod.Create:
            case ApiMethod.Post:
            case ApiMethod.PostWithResult:
                parts.Add($"{route.TypeName} request");
                break;

            case ApiMethod.GetOrNotFound:
            case ApiMethod.Delete:
                foreach (var rp in route.RouteParams)
                {
                    parts.Add($"{rp.TypeName} {ToCamelCase(rp.Name)}");
                }
                break;

            case ApiMethod.Update:
            case ApiMethod.Patch:
            case ApiMethod.UpdateWithResult:
                foreach (var rp in route.RouteParams)
                {
                    parts.Add($"{rp.TypeName} {ToCamelCase(rp.Name)}");
                }
                parts.Add($"{route.TypeName} request");
                break;

            case ApiMethod.GetGrid:
                parts.Add($"{route.TypeName}? request = null");
                break;

            case ApiMethod.GetCollection:
                break;

            case ApiMethod.Get:
                parts.Add($"{route.TypeName} request");
                break;
        }

        parts.Add("global::System.Threading.CancellationToken ct = default");
        return string.Join(", ", parts);
    }

    private static void GenerateMethodBody(StringBuilder sb, MethodInfo method)
    {
        var route = method.Route;
        var returnType = GetReturnType(route);
        var parameters = GetParameters(route);

        sb.AppendLine($"    public async {returnType} {method.Name}({parameters})");
        sb.AppendLine("    {");

        var routeString = BuildRouteExpression(route);

        switch (route.Method)
        {
            case ApiMethod.Create:
                sb.AppendLine($"        var response = await httpClient.PostAsJsonAsync({routeString}, request, ct);");
                sb.AppendLine("        response.EnsureSuccessStatusCode();");
                sb.AppendLine();
                sb.AppendLine("        var result = await response.Content.ReadFromJsonAsync<CreateEntityResponse>(ct);");
                sb.AppendLine("        return result?.Id ?? throw new global::System.InvalidOperationException(\"Failed to parse response\");");
                break;

            case ApiMethod.GetOrNotFound:
                sb.AppendLine($"        var response = await httpClient.GetAsync({routeString}, ct);");
                sb.AppendLine();
                sb.AppendLine("        if (response.StatusCode == HttpStatusCode.NotFound)");
                sb.AppendLine("            return null;");
                sb.AppendLine();
                sb.AppendLine("        response.EnsureSuccessStatusCode();");
                sb.AppendLine($"        return await response.Content.ReadFromJsonAsync<{route.ResponseTypeName}>(ct);");
                break;

            case ApiMethod.GetGrid:
                sb.AppendLine($"        var url = GridRequestUrlBuilder.BuildUrl({routeString}, request);");
                sb.AppendLine("        var response = await httpClient.GetAsync(url, ct);");
                sb.AppendLine("        response.EnsureSuccessStatusCode();");
                sb.AppendLine();
                sb.AppendLine($"        var result = await response.Content.ReadFromJsonAsync<global::Cheetah.Contracts.Responses.GridResult<{route.ResponseTypeName}>>(ct);");
                sb.AppendLine($"        return result ?? new global::Cheetah.Contracts.Responses.GridResult<{route.ResponseTypeName}>();");
                break;

            case ApiMethod.GetCollection:
                sb.AppendLine($"        var response = await httpClient.GetAsync({routeString}, ct);");
                sb.AppendLine("        response.EnsureSuccessStatusCode();");
                sb.AppendLine();
                sb.AppendLine($"        var entities = await response.Content.ReadFromJsonAsync<global::System.Collections.Generic.List<{route.ResponseTypeName}>>(ct);");
                sb.AppendLine("        return entities ?? [];");
                break;

            case ApiMethod.Get:
                sb.AppendLine($"        var response = await httpClient.GetAsync({routeString}, ct);");
                sb.AppendLine("        response.EnsureSuccessStatusCode();");
                sb.AppendLine();
                sb.AppendLine($"        return await response.Content.ReadFromJsonAsync<{route.ResponseTypeName}>(ct)");
                sb.AppendLine("            ?? throw new global::System.InvalidOperationException(\"Failed to parse response\");");
                break;

            case ApiMethod.Post:
                sb.AppendLine($"        var response = await httpClient.PostAsJsonAsync({routeString}, request, ct);");
                sb.AppendLine("        response.EnsureSuccessStatusCode();");
                break;

            case ApiMethod.PostWithResult:
                sb.AppendLine($"        var response = await httpClient.PostAsJsonAsync({routeString}, request, ct);");
                sb.AppendLine("        response.EnsureSuccessStatusCode();");
                sb.AppendLine();
                sb.AppendLine($"        return await response.Content.ReadFromJsonAsync<{route.ResponseTypeName}>(ct)");
                sb.AppendLine("            ?? throw new global::System.InvalidOperationException(\"Failed to parse response\");");
                break;

            case ApiMethod.Update:
                sb.AppendLine($"        var response = await httpClient.PutAsJsonAsync({routeString}, request, ct);");
                sb.AppendLine("        response.EnsureSuccessStatusCode();");
                break;

            case ApiMethod.UpdateWithResult:
                sb.AppendLine($"        var response = await httpClient.PutAsJsonAsync({routeString}, request, ct);");
                sb.AppendLine("        response.EnsureSuccessStatusCode();");
                sb.AppendLine();
                sb.AppendLine($"        return await response.Content.ReadFromJsonAsync<{route.ResponseTypeName}>(ct)");
                sb.AppendLine("            ?? throw new global::System.InvalidOperationException(\"Failed to parse response\");");
                break;

            case ApiMethod.Delete:
                sb.AppendLine($"        var response = await httpClient.DeleteAsync({routeString}, ct);");
                sb.AppendLine("        response.EnsureSuccessStatusCode();");
                break;

            case ApiMethod.Patch:
                sb.AppendLine($"        var response = await httpClient.PatchAsJsonAsync({routeString}, request, ct);");
                sb.AppendLine("        response.EnsureSuccessStatusCode();");
                break;
        }

        sb.AppendLine("    }");
    }

    private static string BuildRouteExpression(ApiRouteInfo route)
    {
        var template = route.Route;

        if (route.RouteParams.Length == 0)
        {
            return $"\"{template}\"";
        }

        var result = template;
        foreach (var param in route.RouteParams)
        {
            var paramNameLower = param.Name.ToLowerInvariant();
            result = System.Text.RegularExpressions.Regex.Replace(
                result,
                @"\{" + paramNameLower + @"(:[^}]*)?\}",
                "{" + ToCamelCase(param.Name) + "}",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        return "$\"" + result + "\"";
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;
        return char.ToLowerInvariant(name[0]) + name.Substring(1);
    }

    // Data models
    private sealed record ModuleInfo(string ClassName, string Namespace, string ServiceName);

    private sealed record ApiRouteInfo(
        string TypeName,
        string ShortTypeName,
        string Route,
        ApiMethod Method,
        string? ResponseTypeName,
        string? MethodName,
        string? ServiceName,
        RouteParamInfo[] RouteParams,
        bool IsGridRequest);

    private sealed record RouteParamInfo(string Name, string TypeName);

    private sealed record MethodInfo(string Name, ApiRouteInfo Route);

    private sealed record ServiceRegistration(string InterfaceName, string ClassName);
}
