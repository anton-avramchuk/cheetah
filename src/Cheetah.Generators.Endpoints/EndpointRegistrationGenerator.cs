using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Cheetah.Generators.Endpoints;

[Generator]
public class EndpointRegistrationGenerator : IIncrementalGenerator
{
    private const string ErrorCode = "ENDPGEN001";
    private const string ErrorCategory = nameof(EndpointRegistrationGenerator);
    private const string GridResultTypeName = "Cheetah.Contracts.Responses.GridResult";
    private const string GridRequestTypeName = "Cheetah.Contracts.Requests.GridRequest";

    // Base endpoint types to look for
    private static readonly string[] EndpointBaseTypes = new[]
    {
        "Cheetah.Backend.Endpoints.Http.QueryEndpoint`4",
        "Cheetah.Backend.Endpoints.Http.QueryOrNotFoundEndpoint`4",
        "Cheetah.Backend.Endpoints.Http.QueryCollectionEndpoint`4",
        "Cheetah.Backend.Endpoints.Http.QueryGridEndpoint`4",
        "Cheetah.Backend.Endpoints.Http.CommandEndpoint`2",
        "Cheetah.Backend.Endpoints.Http.CommandWithResultEndpoint`4",
        "Cheetah.Backend.Endpoints.Http.CreateCommandEndpoint`2",
        "Cheetah.Backend.Endpoints.Http.UpdateCommandEndpoint`2",
        "Cheetah.Backend.Endpoints.Http.UpdateCommandWithResultEndpoint`4",
        "Cheetah.Backend.Endpoints.Http.PatchCommandEndpoint`2",
        "Cheetah.Backend.Endpoints.Http.DeleteCommandEndpoint`2",
        "Cheetah.Backend.Endpoints.Http.DeleteCommandWithResultEndpoint`4",
        "Cheetah.Backend.Endpoints.Http.UploadCommandEndpoint`4"
    };

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all classes that inherit from endpoint base classes
        var endpointClasses = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsClassDeclaration(node),
                transform: static (context, _) => GetEndpointClass(context))
            .Where(static endpoint => endpoint is not null)
            .Collect();

        context.RegisterSourceOutput(endpointClasses, (ctx, endpoints) =>
        {
            if (endpoints.IsDefaultOrEmpty)
                return;

            // Group endpoints by module
            var endpointsByModule = GroupEndpointsByModule(endpoints!);

            foreach (var kvp in endpointsByModule)
            {
                GenerateModuleEndpointsCode(ctx, kvp.Key, kvp.Value);
            }
        });
    }

    private static bool IsClassDeclaration(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax classDecl &&
               !classDecl.Modifiers.Any(m => m.ValueText == "abstract");
    }

    private static EndpointInfo? GetEndpointClass(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;

        if (classSymbol is null || classSymbol.IsAbstract)
            return null;

        // Check if inherits from any endpoint base type
        var baseType = GetEndpointBaseType(classSymbol);
        if (baseType is null)
            return null;

        return new EndpointInfo(classSymbol, baseType);
    }

    private static INamedTypeSymbol? GetEndpointBaseType(INamedTypeSymbol classSymbol)
    {
        var currentType = classSymbol.BaseType;
        while (currentType is not null)
        {
            var baseTypeName = GetGenericTypeName(currentType);
            if (EndpointBaseTypes.Contains(baseTypeName))
                return currentType;

            currentType = currentType.BaseType;
        }
        return null;
    }

    private static string GetGenericTypeName(INamedTypeSymbol type)
    {
        if (!type.IsGenericType)
            return type.ToDisplayString();

        var typeNamespace = type.ContainingNamespace.ToDisplayString();
        var typeName = type.Name;
        var arity = type.TypeArguments.Length;

        return $"{typeNamespace}.{typeName}`{arity}";
    }

    private static Dictionary<INamedTypeSymbol, List<EndpointInfo>> GroupEndpointsByModule(
        ImmutableArray<EndpointInfo> endpoints)
    {
        var result = new Dictionary<INamedTypeSymbol, List<EndpointInfo>>(SymbolEqualityComparer.Default);

        foreach (var endpoint in endpoints)
        {
            var moduleSymbol = FindModuleInAssembly(endpoint.ClassSymbol.ContainingAssembly);
            if (moduleSymbol is null)
                continue;

            if (!result.TryGetValue(moduleSymbol, out var list))
            {
                list = new List<EndpointInfo>();
                result[moduleSymbol] = list;
            }

            list.Add(endpoint);
        }

        return result;
    }

    private static INamedTypeSymbol? FindModuleInAssembly(IAssemblySymbol assembly)
    {
        return FindModuleInNamespace(assembly.GlobalNamespace);
    }

    private static INamedTypeSymbol? FindModuleInNamespace(INamespaceSymbol namespaceSymbol)
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

    private static void GenerateModuleEndpointsCode(
        SourceProductionContext ctx,
        INamedTypeSymbol moduleSymbol,
        List<EndpointInfo> endpoints)
    {
        var sb = new StringBuilder();

        // File header
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();

        // Usings
        sb.AppendLine("using Microsoft.AspNetCore.Builder;");
        sb.AppendLine("using Microsoft.AspNetCore.Http;");
        sb.AppendLine("using Microsoft.AspNetCore.Mvc;");
        sb.AppendLine("using Microsoft.AspNetCore.Routing;");
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine("using Cheetah.AspNetCore.Extensions;");
        sb.AppendLine("using Cheetah.AspNetCore.Contracts.Extensions;");
        sb.AppendLine("using Cheetah.Core;");
        sb.AppendLine("using Cheetah.Core.CQRS;");
        sb.AppendLine("using Cheetah.Mapping.Core;");
        sb.AppendLine("using Cheetah.Backend.Endpoints.Abstractions;");
        sb.AppendLine();

        // Namespace
        sb.AppendLine($"namespace {moduleSymbol.ContainingNamespace};");
        sb.AppendLine();

        // Partial class
        sb.AppendLine($"partial class {moduleSymbol.Name}");
        sb.AppendLine("{");

        // OnApplicationInitialization method
        sb.AppendLine("    public override void OnApplicationInitialization(ApplicationInitializationContext context)");
        sb.AppendLine("    {");
        sb.AppendLine("        var routeBuilder = context.GetRouteBuilder();");
        sb.AppendLine("        var serviceProvider = context.ServiceProvider;");
        sb.AppendLine();
        sb.AppendLine("        RegisterGeneratedEndpoints(routeBuilder, serviceProvider);");
        sb.AppendLine();
        sb.AppendLine("        // Call custom initialization logic if defined");
        sb.AppendLine("        OnApplicationInitializationCustom(context);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Optional partial method for custom initialization logic.");
        sb.AppendLine("    /// Implement this method in your partial class to add custom endpoint registrations or other initialization logic.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    partial void OnApplicationInitializationCustom(ApplicationInitializationContext context);");
        sb.AppendLine();

        // RegisterGeneratedEndpoints method
        sb.AppendLine("    private void RegisterGeneratedEndpoints(IEndpointRouteBuilder routeBuilder, IServiceProvider serviceProvider)");
        sb.AppendLine("    {");

        // Generate registration for each endpoint
        foreach (var endpoint in endpoints)
        {
            GenerateEndpointRegistration(sb, endpoint);
        }

        sb.AppendLine("    }");
        sb.AppendLine();

        // ApplyMetadata helper method
        GenerateApplyMetadataMethod(sb);

        sb.AppendLine("}");

        var source = sb.ToString();
        ctx.AddSource($"{moduleSymbol.Name}.Endpoints.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private static void GenerateEndpointRegistration(StringBuilder sb, EndpointInfo endpoint)
    {
        var className = endpoint.ClassSymbol.Name;
        var baseTypeName = GetGenericTypeName(endpoint.BaseType).Split('`')[0].Split('.').Last();

        sb.AppendLine($"        // {className} - {baseTypeName}");
        sb.AppendLine("        {");
        sb.AppendLine($"            var endpoint = new {endpoint.ClassSymbol.ToDisplayString()}();");

        var (httpMethod, handlerCode, producesCode) = GenerateHandlerCode(endpoint);

        sb.AppendLine($"            var builder = routeBuilder.{httpMethod}(endpoint.Route, async ({handlerCode.parameters}) =>");
        sb.AppendLine("            {");

        foreach (var line in handlerCode.body)
        {
            sb.AppendLine($"                {line}");
        }

        sb.AppendLine("            });");
        sb.AppendLine();
        sb.AppendLine("            ApplyMetadata(builder, endpoint);");

        // Add Produces metadata
        foreach (var produces in producesCode)
        {
            sb.AppendLine($"            {produces}");
        }

        sb.AppendLine("        }");
        sb.AppendLine();
    }

    private static (string httpMethod, (string parameters, List<string> body) handler, List<string> produces)
        GenerateHandlerCode(EndpointInfo endpoint)
    {
        var baseTypeName = GetGenericTypeName(endpoint.BaseType);
        var typeArgs = endpoint.BaseType.TypeArguments;

        return baseTypeName switch
        {
            "Cheetah.Backend.Endpoints.Http.QueryEndpoint`4" =>
                GenerateQueryEndpoint(typeArgs),

            "Cheetah.Backend.Endpoints.Http.QueryOrNotFoundEndpoint`4" =>
                GenerateQueryOrNotFoundEndpoint(typeArgs),

            "Cheetah.Backend.Endpoints.Http.QueryCollectionEndpoint`4" =>
                GenerateQueryCollectionEndpoint(typeArgs),

            "Cheetah.Backend.Endpoints.Http.QueryGridEndpoint`4" =>
                GenerateQueryGridEndpoint(typeArgs),

            "Cheetah.Backend.Endpoints.Http.CommandEndpoint`2" =>
                GenerateCommandEndpoint(typeArgs),

            "Cheetah.Backend.Endpoints.Http.CommandWithResultEndpoint`4" =>
                GenerateCommandWithResultEndpoint(typeArgs),

            "Cheetah.Backend.Endpoints.Http.CreateCommandEndpoint`2" =>
                GenerateCreateCommandEndpoint(typeArgs),

            "Cheetah.Backend.Endpoints.Http.UpdateCommandEndpoint`2" =>
                GenerateUpdateCommandEndpoint(typeArgs),

            "Cheetah.Backend.Endpoints.Http.UpdateCommandWithResultEndpoint`4" =>
                GenerateUpdateCommandWithResultEndpoint(typeArgs),

            "Cheetah.Backend.Endpoints.Http.PatchCommandEndpoint`2" =>
                GeneratePatchCommandEndpoint(typeArgs),

            "Cheetah.Backend.Endpoints.Http.DeleteCommandEndpoint`2" =>
                GenerateDeleteCommandEndpoint(typeArgs),

            "Cheetah.Backend.Endpoints.Http.DeleteCommandWithResultEndpoint`4" =>
                GenerateDeleteCommandWithResultEndpoint(typeArgs),

            "Cheetah.Backend.Endpoints.Http.UploadCommandEndpoint`4" =>
                GenerateUploadCommandEndpoint(typeArgs),

            _ => throw new InvalidOperationException($"Unknown endpoint type: {baseTypeName}")
        };
    }


    /// <summary>
    /// Наследуется ли запрос от <c>GridRequest</c>. Такой запрос несёт фильтр и сортировку деревом
    /// (<c>filter[filters][0][field]=...</c>), а это не биндится штатным разбором строки запроса: сложное
    /// свойство ASP.NET уводит в тело, и GET-эндпоинт роняет приложение на старте ("Body was inferred").
    /// </summary>
    private static bool IsGridRequest(ITypeSymbol request)
    {
        for (var type = request as INamedTypeSymbol; type != null; type = type.BaseType)
        {
            if (type.ToDisplayString() == GridRequestTypeName)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Параметры и первая строка тела для GET-эндпоинта: грид-запрос разбирается из HttpContext, всё
    /// остальное — штатным <c>[AsParameters]</c>. Так любой GET умеет принимать грид-фильтр, а не только
    /// список: тем же фильтром считаются, например, счётчики значений фильтров рядом со списком.
    /// </summary>
    private static (string Parameters, string? Binding) GetRequestBinding(ITypeSymbol request)
    {
        var tRequest = request.ToDisplayString();
        var services = "[FromServices] IDispatcher dispatcher, [FromServices] IObjectMapper mapper, CancellationToken cancellationToken";

        return IsGridRequest(request)
            ? ($"HttpContext httpContext, {services}", $"var request = httpContext.BindGridRequest<{tRequest}>();")
            : ($"[AsParameters] {tRequest} request, {services}", null);
    }

    private static (string, (string, List<string>), List<string>) GenerateQueryEndpoint(ImmutableArray<ITypeSymbol> typeArgs)
    {
        var tQuery = typeArgs[1].ToDisplayString();
        var tQueryResult = typeArgs[2].ToDisplayString();
        var tResponse = typeArgs[3].ToDisplayString();

        var (parameters, binding) = GetRequestBinding(typeArgs[0]);
        var body = new List<string>();

        if (binding != null)
            body.Add(binding);

        body.AddRange(new[]
        {
            $"var query = mapper.Map<{tQuery}>(request);",
            $"var result = await dispatcher.QueryAsync<{tQuery}, {tQueryResult}>(query, cancellationToken);",
            $"var response = mapper.Map<{tResponse}>(result);",
            "return Results.Ok(response);"
        });

        var produces = new List<string>
        {
            $"builder.Produces<{tResponse}>(StatusCodes.Status200OK);"
        };

        return ("MapGet", (parameters, body), produces);
    }

    private static (string, (string, List<string>), List<string>) GenerateQueryOrNotFoundEndpoint(ImmutableArray<ITypeSymbol> typeArgs)
    {
        var tQuery = typeArgs[1].ToDisplayString();
        var tQueryResult = typeArgs[2].ToDisplayString();
        var tResponse = typeArgs[3].ToDisplayString();

        // QueryOrNotFoundEndpoint declares TQuery : IQuery<TQueryResult?>, so the dispatcher
        // call must use the nullable result type to match the IQuery<T> constraint exactly.
        var tQueryResultNullable = tQueryResult.EndsWith("?") ? tQueryResult : $"{tQueryResult}?";

        var (parameters, binding) = GetRequestBinding(typeArgs[0]);
        var body = new List<string>();

        if (binding != null)
            body.Add(binding);

        body.AddRange(new[]
        {
            $"var query = mapper.Map<{tQuery}>(request);",
            $"var result = await dispatcher.QueryAsync<{tQuery}, {tQueryResultNullable}>(query, cancellationToken);",
            "",
            "if (result == null)",
            "    return Results.NotFound();",
            "",
            $"var response = mapper.Map<{tResponse}>(result);",
            "return Results.Ok(response);"
        });

        var produces = new List<string>
        {
            $"builder.Produces<{tResponse}>(StatusCodes.Status200OK);",
            "builder.Produces(StatusCodes.Status404NotFound);"
        };

        return ("MapGet", (parameters, body), produces);
    }

    private static (string, (string, List<string>), List<string>) GenerateQueryCollectionEndpoint(ImmutableArray<ITypeSymbol> typeArgs)
    {
        var tQuery = typeArgs[1].ToDisplayString();
        var tQueryResult = typeArgs[2].ToDisplayString();
        var tResponse = typeArgs[3].ToDisplayString();

        var (parameters, binding) = GetRequestBinding(typeArgs[0]);
        var body = new List<string>();

        if (binding != null)
            body.Add(binding);

        body.AddRange(new[]
        {
            $"var query = mapper.Map<{tQuery}>(request);",
            $"var results = await dispatcher.QueryAsync<{tQuery}, System.Collections.Generic.IReadOnlyList<{GetCollectionItemType(tQueryResult)}>>(query, cancellationToken);",
            $"var responses = mapper.Map<System.Collections.Generic.IReadOnlyList<{tResponse}>>(results);",
            "return Results.Ok(responses);"
        });

        var produces = new List<string>
        {
            $"builder.Produces<System.Collections.Generic.IReadOnlyList<{tResponse}>>(StatusCodes.Status200OK);"
        };

        return ("MapGet", (parameters, body), produces);
    }

    private static (string, (string, List<string>), List<string>) GenerateQueryGridEndpoint(ImmutableArray<ITypeSymbol> typeArgs)
    {
        var tRequest = typeArgs[0].ToDisplayString();
        var tQuery = typeArgs[1].ToDisplayString();
        var tQueryResult = typeArgs[2].ToDisplayString();
        var tResponse = typeArgs[3].ToDisplayString();

        var queryGridResultType = $"{GridResultTypeName}<{tQueryResult}>";
        var responseGridResultType = $"{GridResultTypeName}<{tResponse}>";

        // Use HttpContext and BindGridRequest extension for complex query string binding
        var parameters = $"HttpContext httpContext, [FromServices] IDispatcher dispatcher, [FromServices] IObjectMapper mapper, CancellationToken cancellationToken";
        var body = new List<string>
        {
            $"var request = httpContext.BindGridRequest<{tRequest}>();",
            $"var query = mapper.Map<{tQuery}>(request);",
            $"var result = await dispatcher.QueryAsync<{tQuery}, {queryGridResultType}>(query, cancellationToken);",
            $"var responseItems = mapper.Map<System.Collections.Generic.IReadOnlyList<{tResponse}>>(result.Data);",
            $"var response = new {responseGridResultType}(responseItems, result.Total);",
            "return Results.Ok(response);"
        };

        var produces = new List<string>
        {
            $"builder.Produces<{responseGridResultType}>(StatusCodes.Status200OK);"
        };

        return ("MapGet", (parameters, body), produces);
    }

    private static (string, (string, List<string>), List<string>) GenerateCommandEndpoint(ImmutableArray<ITypeSymbol> typeArgs)
    {
        var tRequest = typeArgs[0].ToDisplayString();
        var tCommand = typeArgs[1].ToDisplayString();

        // EmptyBodyBehavior.Allow: подэкшены без тела (только route-параметры) не должны падать 400.
        var parameters = $"[FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] {tRequest} bodyRequest, HttpContext httpContext, [FromServices] IDispatcher dispatcher, [FromServices] IObjectMapper mapper, CancellationToken cancellationToken";
        var body = new List<string>
        {
            "var request = httpContext.MergeRouteValuesInto(bodyRequest);",
            $"var command = mapper.Map<{tCommand}>(request);",
            "await dispatcher.SendAsync(command, cancellationToken);",
            "return Results.NoContent();"
        };

        var produces = new List<string>
        {
            "builder.Produces(StatusCodes.Status204NoContent);",
            "builder.Produces(StatusCodes.Status400BadRequest);"
        };

        return ("MapPost", (parameters, body), produces);
    }

    private static (string, (string, List<string>), List<string>) GenerateCommandWithResultEndpoint(ImmutableArray<ITypeSymbol> typeArgs)
    {
        var tRequest = typeArgs[0].ToDisplayString();
        var tCommand = typeArgs[1].ToDisplayString();
        var tCommandResult = typeArgs[2].ToDisplayString();
        var tResponse = typeArgs[3].ToDisplayString();

        // EmptyBodyBehavior.Allow: подэкшены без тела (только route-параметры) не должны падать 400.
        var parameters = $"[FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] {tRequest} bodyRequest, HttpContext httpContext, [FromServices] IDispatcher dispatcher, [FromServices] IObjectMapper mapper, CancellationToken cancellationToken";
        var body = new List<string>
        {
            "var request = httpContext.MergeRouteValuesInto(bodyRequest);",
            $"var command = mapper.Map<{tCommand}>(request);",
            $"var result = await dispatcher.SendAsync<{tCommand}, {tCommandResult}>(command, cancellationToken);",
            $"var response = mapper.Map<{tResponse}>(result);",
            "return Results.Ok(response);"
        };

        var produces = new List<string>
        {
            $"builder.Produces<{tResponse}>(StatusCodes.Status200OK);",
            "builder.Produces(StatusCodes.Status400BadRequest);"
        };

        return ("MapPost", (parameters, body), produces);
    }

    private static (string, (string, List<string>), List<string>) GenerateCreateCommandEndpoint(ImmutableArray<ITypeSymbol> typeArgs)
    {
        var tRequest = typeArgs[0].ToDisplayString();
        var tCommand = typeArgs[1].ToDisplayString();

        var parameters = $"[FromBody] {tRequest} bodyRequest, HttpContext httpContext, [FromServices] IDispatcher dispatcher, [FromServices] IObjectMapper mapper, CancellationToken cancellationToken";
        var body = new List<string>
        {
            $"var request = httpContext.MergeRouteValuesInto(bodyRequest);",
            $"var command = mapper.Map<{tCommand}>(request);",
            $"var id = await dispatcher.SendAsync<{tCommand}, System.Guid>(command, cancellationToken);",
            "var routeValues = new Microsoft.AspNetCore.Routing.RouteValueDictionary(httpContext.Request.RouteValues) { [\"id\"] = id };",
            "return Results.CreatedAtRoute(endpoint.GetByIdRouteName, routeValues, new Cheetah.Backend.Endpoints.Responses.GuidResponse(id));"
        };

        var produces = new List<string>
        {
            "builder.Produces<Cheetah.Backend.Endpoints.Responses.GuidResponse>(StatusCodes.Status201Created);",
            "builder.Produces(StatusCodes.Status400BadRequest);"
        };

        return ("MapPost", (parameters, body), produces);
    }

    private static (string, (string, List<string>), List<string>) GenerateUpdateCommandEndpoint(ImmutableArray<ITypeSymbol> typeArgs)
    {
        var tRequest = typeArgs[0].ToDisplayString();
        var tCommand = typeArgs[1].ToDisplayString();

        var parameters = $"[FromBody] {tRequest} bodyRequest, HttpContext httpContext, [FromServices] IDispatcher dispatcher, [FromServices] IObjectMapper mapper, CancellationToken cancellationToken";
        var body = new List<string>
        {
            $"var request = httpContext.MergeRouteValuesInto(bodyRequest);",
            $"var command = mapper.Map<{tCommand}>(request);",
            "await dispatcher.SendAsync(command, cancellationToken);",
            "return Results.NoContent();"
        };

        var produces = new List<string>
        {
            "builder.Produces(StatusCodes.Status204NoContent);",
            "builder.Produces(StatusCodes.Status404NotFound);",
            "builder.Produces(StatusCodes.Status400BadRequest);"
        };

        return ("MapPut", (parameters, body), produces);
    }

    private static (string, (string, List<string>), List<string>) GenerateUpdateCommandWithResultEndpoint(ImmutableArray<ITypeSymbol> typeArgs)
    {
        var tRequest = typeArgs[0].ToDisplayString();
        var tCommand = typeArgs[1].ToDisplayString();
        var tCommandResult = typeArgs[2].ToDisplayString();
        var tResponse = typeArgs[3].ToDisplayString();

        var parameters = $"[FromBody] {tRequest} bodyRequest, HttpContext httpContext, [FromServices] IDispatcher dispatcher, [FromServices] IObjectMapper mapper, CancellationToken cancellationToken";
        var body = new List<string>
        {
            $"var request = httpContext.MergeRouteValuesInto(bodyRequest);",
            $"var command = mapper.Map<{tCommand}>(request);",
            $"var result = await dispatcher.SendAsync<{tCommand}, {tCommandResult}>(command, cancellationToken);",
            $"var response = mapper.Map<{tResponse}>(result);",
            "return Results.Ok(response);"
        };

        var produces = new List<string>
        {
            $"builder.Produces<{tResponse}>(StatusCodes.Status200OK);",
            "builder.Produces(StatusCodes.Status404NotFound);",
            "builder.Produces(StatusCodes.Status400BadRequest);"
        };

        return ("MapPut", (parameters, body), produces);
    }

    private static (string, (string, List<string>), List<string>) GeneratePatchCommandEndpoint(ImmutableArray<ITypeSymbol> typeArgs)
    {
        var tRequest = typeArgs[0].ToDisplayString();
        var tCommand = typeArgs[1].ToDisplayString();

        var parameters = $"[FromBody] {tRequest} bodyRequest, HttpContext httpContext, [FromServices] IDispatcher dispatcher, [FromServices] IObjectMapper mapper, CancellationToken cancellationToken";
        var body = new List<string>
        {
            $"var request = httpContext.MergeRouteValuesInto(bodyRequest);",
            $"var command = mapper.Map<{tCommand}>(request);",
            "await dispatcher.SendAsync(command, cancellationToken);",
            "return Results.NoContent();"
        };

        var produces = new List<string>
        {
            "builder.Produces(StatusCodes.Status204NoContent);",
            "builder.Produces(StatusCodes.Status404NotFound);",
            "builder.Produces(StatusCodes.Status400BadRequest);"
        };

        return ("MapPatch", (parameters, body), produces);
    }

    private static (string, (string, List<string>), List<string>) GenerateDeleteCommandEndpoint(ImmutableArray<ITypeSymbol> typeArgs)
    {
        var tRequest = typeArgs[0].ToDisplayString();
        var tCommand = typeArgs[1].ToDisplayString();

        var parameters = $"[AsParameters] {tRequest} request, [FromServices] IDispatcher dispatcher, [FromServices] IObjectMapper mapper, CancellationToken cancellationToken";
        var body = new List<string>
        {
            $"var command = mapper.Map<{tCommand}>(request);",
            "await dispatcher.SendAsync(command, cancellationToken);",
            "return Results.NoContent();"
        };

        var produces = new List<string>
        {
            "builder.Produces(StatusCodes.Status204NoContent);",
            "builder.Produces(StatusCodes.Status404NotFound);"
        };

        return ("MapDelete", (parameters, body), produces);
    }

    /// <summary>
    /// DELETE с телом ответа: удаление, у которого есть отмена, обязано вернуть и состояние после
    /// себя, и токен отката — иначе вызывающий дочитывал бы их вторым запросом.
    /// </summary>
    private static (string, (string, List<string>), List<string>) GenerateDeleteCommandWithResultEndpoint(ImmutableArray<ITypeSymbol> typeArgs)
    {
        var tRequest = typeArgs[0].ToDisplayString();
        var tCommand = typeArgs[1].ToDisplayString();
        var tCommandResult = typeArgs[2].ToDisplayString();
        var tResponse = typeArgs[3].ToDisplayString();

        var parameters = $"[AsParameters] {tRequest} request, [FromServices] IDispatcher dispatcher, [FromServices] IObjectMapper mapper, CancellationToken cancellationToken";
        var body = new List<string>
        {
            $"var command = mapper.Map<{tCommand}>(request);",
            $"var result = await dispatcher.SendAsync<{tCommand}, {tCommandResult}>(command, cancellationToken);",
            $"var response = mapper.Map<{tResponse}>(result);",
            "return Results.Ok(response);"
        };

        var produces = new List<string>
        {
            $"builder.Produces<{tResponse}>(StatusCodes.Status200OK);",
            "builder.Produces(StatusCodes.Status404NotFound);"
        };

        return ("MapDelete", (parameters, body), produces);
    }

    /// <summary>
    /// Загрузка файла: запрос приезжает формой (multipart/form-data), а не телом JSON — иначе файл
    /// в него не положить. Дальше всё как у обычной команды с результатом.
    ///
    /// <c>DisableAntiforgery</c> обязателен: метаданные формы требуют middleware защиты от подделки,
    /// и без него маршрут отвечает 500 ещё до обработчика.
    /// </summary>
    private static (string, (string, List<string>), List<string>) GenerateUploadCommandEndpoint(ImmutableArray<ITypeSymbol> typeArgs)
    {
        var tRequest = typeArgs[0].ToDisplayString();
        var tCommand = typeArgs[1].ToDisplayString();
        var tCommandResult = typeArgs[2].ToDisplayString();
        var tResponse = typeArgs[3].ToDisplayString();

        var parameters = $"[FromForm] {tRequest} request, [FromServices] IDispatcher dispatcher, [FromServices] IObjectMapper mapper, CancellationToken cancellationToken";
        var body = new List<string>
        {
            $"var command = mapper.Map<{tCommand}>(request);",
            $"var result = await dispatcher.SendAsync<{tCommand}, {tCommandResult}>(command, cancellationToken);",
            $"var response = mapper.Map<{tResponse}>(result);",
            "return Results.Ok(response);"
        };

        var produces = new List<string>
        {
            "builder.DisableAntiforgery();",
            $"builder.Produces<{tResponse}>(StatusCodes.Status200OK);",
            "builder.Produces(StatusCodes.Status400BadRequest);"
        };

        return ("MapPost", (parameters, body), produces);
    }

    private static string GetCollectionItemType(string collectionType)
    {
        // Extract T from IReadOnlyList<T>
        var start = collectionType.IndexOf('<') + 1;
        var end = collectionType.LastIndexOf('>');
        if (start > 0 && end > start)
        {
            return collectionType.Substring(start, end - start);
        }
        return collectionType;
    }

    private static void GenerateApplyMetadataMethod(StringBuilder sb)
    {
        sb.AppendLine("    private static void ApplyMetadata(RouteHandlerBuilder builder, IEndpointDefinition endpoint)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (!string.IsNullOrEmpty(endpoint.Name))");
        sb.AppendLine("            builder.WithName(endpoint.Name);");
        sb.AppendLine();
        sb.AppendLine("        if (!string.IsNullOrEmpty(endpoint.Description))");
        sb.AppendLine("            builder.WithDescription(endpoint.Description);");
        sb.AppendLine();
        sb.AppendLine("        if (!string.IsNullOrEmpty(endpoint.Summary))");
        sb.AppendLine("            builder.WithSummary(endpoint.Summary);");
        sb.AppendLine();
        sb.AppendLine("        if (endpoint.Tags.Length > 0)");
        sb.AppendLine("            builder.WithTags(endpoint.Tags);");
        sb.AppendLine();
        sb.AppendLine("        if (endpoint.AllowAnonymous)");
        sb.AppendLine("            builder.AllowAnonymous();");
        sb.AppendLine();
        sb.AppendLine("        foreach (var policy in endpoint.AuthorizationPolicies)");
        sb.AppendLine("            builder.RequireAuthorization(policy);");
        sb.AppendLine();
        sb.AppendLine("        if (endpoint.IsDeprecated)");
        sb.AppendLine("            builder.WithMetadata(new System.ObsoleteAttribute(\"This endpoint is deprecated\"));");
        sb.AppendLine();
        sb.AppendLine("        if (endpoint.CacheControl is { } cache)");
        sb.AppendLine("        {");
        sb.AppendLine("            builder.AddEndpointFilter(async (ctx, next) =>");
        sb.AppendLine("            {");
        sb.AppendLine("                var result = await next(ctx);");
        sb.AppendLine("                ctx.HttpContext.Response.Headers.CacheControl = cache.NoStore");
        sb.AppendLine("                    ? \"no-store, no-cache\"");
        sb.AppendLine("                    : (cache.IsPublic ? \"public\" : \"private\") + \", max-age=\" + cache.MaxAgeSeconds;");
        sb.AppendLine("                return result;");
        sb.AppendLine("            });");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        if (endpoint.RateLimit is { } rateLimit)");
        sb.AppendLine("        {");
        sb.AppendLine("            builder.AddEndpointFilter(new global::Cheetah.Backend.Endpoints.Http.RateLimitFilter(rateLimit));");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        if (!string.IsNullOrEmpty(endpoint.FeatureKey))");
        sb.AppendLine("        {");
        sb.AppendLine("            builder.AddEndpointFilter(new global::Cheetah.Backend.Endpoints.Http.FeatureGateFilter(endpoint.FeatureKey!));");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
    }

    private record EndpointInfo(INamedTypeSymbol ClassSymbol, INamedTypeSymbol BaseType);
}
