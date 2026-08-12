using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Extensions;
using Cheetah.Core;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.OpenApi;

namespace Cheetah.OpenApi;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmAspNetCoreModule))]
public partial class OpenApiModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        var configuration = context.Services.GetConfiguration();
        var basePath = configuration["OpenApi:BasePath"] ?? "";

        // Имя документа настраивается: сервис, у которого документ называется иначе (BFF отдаёт
        // «crm», под этим именем спека попадает во фронт), иначе получил бы ВТОРОЙ документ «v1» с
        // теми же операциями — лишний файл при генерации спеки и лишняя страница в UI.
        var documentName = configuration["OpenApi:DocumentName"] ?? OpenApiConstants.DefaultDocumentName;

        // Оба обогащения по умолчанию включены — они и есть смысл модуля. Выключаются там, где
        // документ описывает чужой контракт: у BFF списки принимают плоские параметры, и восемь
        // grid-параметров в спеке означали бы восемь бесполезных аргументов в клиенте SPA.
        var gridParameters = configuration.GetValue("OpenApi:GridParameters", true);
        var schemaExamples = configuration.GetValue("OpenApi:SchemaExamples", true);

        context.Services.AddOpenApi(documentName, options =>
        {
            options.AddSchemaTransformer((schema, ctx, ct) =>
            {
                if (ctx.JsonTypeInfo.Type == typeof(JsonElement) ||
                    ctx.JsonTypeInfo.Type == typeof(JsonElement?))
                {
                    schema.Type = JsonSchemaType.Object;
                    schema.Properties = null;
                    schema.AdditionalProperties = null;
                }
                return Task.CompletedTask;
            });

            options.AddDocumentTransformer((document, ctx, ct) =>
            {
                if (!string.IsNullOrEmpty(basePath))
                {
                    document.Servers ??= [];
                    document.Servers.Clear();
                    document.Servers.Add(new() { Url = basePath });
                }
                return Task.CompletedTask;
            });

            options.AddOperationTransformer((operation, ctx, ct) =>
            {
                if (!gridParameters || ctx.Description.HttpMethod != "GET")
                    return Task.CompletedTask;

                var isGrid = ctx.Description.ActionDescriptor.EndpointMetadata
                    .OfType<IProducesResponseTypeMetadata>()
                    .Any(m => m.Type is { IsGenericType: true } t
                              && t.GetGenericTypeDefinition().Name == "GridResult`1");

                if (!isGrid)
                    return Task.CompletedTask;

                operation.Parameters ??= [];
                operation.Parameters.Add(GridParam("page",       JsonSchemaType.Integer, "Номер страницы (начиная с 1, по умолчанию: 1)"));
                operation.Parameters.Add(GridParam("pageSize",   JsonSchemaType.Integer, "Размер страницы (0 = все записи, по умолчанию: 10)"));
                operation.Parameters.Add(GridParam("sort[0][field]", JsonSchemaType.String, "Поле сортировки"));
                operation.Parameters.Add(GridParam("sort[0][dir]",   JsonSchemaType.String, "Направление: asc | desc"));
                operation.Parameters.Add(GridParam("filter[logic]",               JsonSchemaType.String, "Логика фильтра: and | or"));
                operation.Parameters.Add(GridParam("filter[filters][0][field]",   JsonSchemaType.String, "Поле фильтра"));
                operation.Parameters.Add(GridParam("filter[filters][0][operator]",JsonSchemaType.String, "Оператор: eq, neq, contains, startswith, endswith, gt, gte, lt, lte"));
                operation.Parameters.Add(GridParam("filter[filters][0][value]",   JsonSchemaType.String, "Значение фильтра"));

                return Task.CompletedTask;
            });

            options.AddSchemaTransformer((schema, ctx, ct) =>
            {
                if (!schemaExamples
                    || schema.Example is not null
                    || schema.Properties is null
                    || schema.Properties.Count == 0)
                    return Task.CompletedTask;

                // Map JSON property name → .NET type for fallback resolution
                // when property schema uses $ref (Type == null, Format == null).
                // Uses reflection instead of JsonTypeInfo.Properties because
                // positional records use constructor-based serialization and
                // JsonTypeInfo.Properties may be empty for them.
                var dotNetProps = ctx.JsonTypeInfo.Type
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .ToDictionary(
                        p => JsonNamingPolicy.CamelCase.ConvertName(p.Name),
                        p => p.PropertyType,
                        StringComparer.OrdinalIgnoreCase);

                var example = new JsonObject();
                foreach (var (name, propSchema) in schema.Properties)
                {
                    dotNetProps.TryGetValue(name, out var dotNetType);
                    example[name] = GetExampleValue(propSchema, dotNetType);
                }
                schema.Example = example;

                return Task.CompletedTask;
            });
        });
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var routeBuilder = context.GetRouteBuilder();
        routeBuilder.MapOpenApi().AllowAnonymous();
    }

    private static OpenApiParameter GridParam(string name, JsonSchemaType type, string description) => new()
    {
        Name = name,
        In = ParameterLocation.Query,
        Required = false,
        Description = description,
        Schema = new OpenApiSchema { Type = type }
    };

    private static JsonNode GetExampleValue(IOpenApiSchema schema, Type? dotNetType = null)
    {
        if (schema.Example is not null)
            return schema.Example.DeepClone();

        // anyOf/oneOf: nullable types in OpenAPI 3.1 → pick first non-null inner schema
        var inner = schema.AnyOf?.FirstOrDefault(s => s.Type != JsonSchemaType.Null)
                 ?? schema.OneOf?.FirstOrDefault(s => s.Type != JsonSchemaType.Null);
        if (inner is not null)
            return GetExampleValue(inner, dotNetType);

        // When schema.Type is null (e.g. $ref component schema), fall back to .NET type
        var type = schema.Type
            ?? DotNetTypeToJsonSchemaType(dotNetType)
            ?? (schema.Format switch
            {
                "int32" or "int64" => JsonSchemaType.Integer,
                "float" or "double" => JsonSchemaType.Number,
                _ => JsonSchemaType.String
            });

        if (type.HasFlag(JsonSchemaType.Integer)) return JsonValue.Create(0)!;
        if (type.HasFlag(JsonSchemaType.Number))  return JsonValue.Create(0.0)!;
        if (type.HasFlag(JsonSchemaType.Boolean)) return JsonValue.Create(false)!;
        if (type.HasFlag(JsonSchemaType.Array))   return new JsonArray();

        if (type.HasFlag(JsonSchemaType.String))
            return schema.Format switch
            {
                "uuid"      => JsonValue.Create(Guid.Empty.ToString())!,
                "date-time" => JsonValue.Create("2025-01-01T00:00:00Z")!,
                "email"     => JsonValue.Create("user@example.com")!,
                _           => JsonValue.Create("string")!,
            };

        if (type.HasFlag(JsonSchemaType.Object))
        {
            var obj = new JsonObject();
            if (schema.Properties is not null)
                foreach (var (name, propSchema) in schema.Properties)
                    obj[name] = GetExampleValue(propSchema);
            return obj;
        }

        return JsonValue.Create(string.Empty)!;
    }

    private static JsonSchemaType? DotNetTypeToJsonSchemaType(Type? type)
    {
        if (type is null) return null;
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        if (underlying == typeof(int)   || underlying == typeof(long)  ||
            underlying == typeof(short) || underlying == typeof(byte)  ||
            underlying == typeof(uint)  || underlying == typeof(ulong) ||
            underlying == typeof(sbyte) || underlying == typeof(ushort))
            return JsonSchemaType.Integer;
        if (underlying == typeof(float) || underlying == typeof(double) || underlying == typeof(decimal))
            return JsonSchemaType.Number;
        if (underlying == typeof(bool))
            return JsonSchemaType.Boolean;
        return null;
    }
}
