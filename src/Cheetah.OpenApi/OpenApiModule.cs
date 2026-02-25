using System.Text.Json.Nodes;
using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Extensions;
using Cheetah.Core;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
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

        context.Services.AddOpenApi(options =>
        {
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

            options.AddSchemaTransformer((schema, ctx, ct) =>
            {
                if (schema.Example is not null
                    || schema.Properties is null
                    || schema.Properties.Count == 0)
                    return Task.CompletedTask;

                var example = new JsonObject();
                foreach (var (name, propSchema) in schema.Properties)
                    example[name] = GetExampleValue(propSchema);
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

    private static JsonNode GetExampleValue(IOpenApiSchema schema)
    {
        if (schema.Example is not null)
            return schema.Example.DeepClone();

        var type = schema.Type ?? JsonSchemaType.String;

        if (type.HasFlag(JsonSchemaType.String))
            return schema.Format switch
            {
                "uuid"      => JsonValue.Create(Guid.Empty.ToString())!,
                "date-time" => JsonValue.Create("2025-01-01T00:00:00Z")!,
                "email"     => JsonValue.Create("user@example.com")!,
                _           => JsonValue.Create("string")!,
            };

        if (type.HasFlag(JsonSchemaType.Integer)) return JsonValue.Create(0)!;
        if (type.HasFlag(JsonSchemaType.Number))  return JsonValue.Create(0.0)!;
        if (type.HasFlag(JsonSchemaType.Boolean)) return JsonValue.Create(false)!;
        if (type.HasFlag(JsonSchemaType.Array))   return new JsonArray();

        if (type.HasFlag(JsonSchemaType.Object) && schema.Properties is not null)
        {
            var obj = new JsonObject();
            foreach (var (name, propSchema) in schema.Properties)
                obj[name] = GetExampleValue(propSchema);
            return obj;
        }

        return JsonValue.Create(string.Empty)!;
    }
}
