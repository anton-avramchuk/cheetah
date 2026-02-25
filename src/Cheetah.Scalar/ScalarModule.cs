using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Extensions;
using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.OpenApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace Cheetah.Scalar;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmAspNetCoreModule), typeof(OpenApiModule))]
public partial class ScalarModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        context.Services.AddOpenApi(options =>
        {
            // Declare the Bearer security scheme in the OpenAPI document
            options.AddDocumentTransformer((document, ctx, ct) =>
            {
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Enter your JWT access token",
                };
                return Task.CompletedTask;
            });

            // Apply Bearer security requirement to all protected operations
            options.AddOperationTransformer((operation, ctx, ct) =>
            {
                var isAnonymous = ctx.Description.ActionDescriptor.EndpointMetadata
                    .OfType<IAllowAnonymous>()
                    .Any();

                if (isAnonymous)
                    return Task.CompletedTask;

                var requirement = new OpenApiSecurityRequirement();
                requirement.Add(
                    new OpenApiSecuritySchemeReference("Bearer", ctx.Document),
                    []);
                operation.Security = [requirement];

                return Task.CompletedTask;
            });
        });
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var routeBuilder = context.GetRouteBuilder();
        var options = context.GetOptions<ScalarModuleOptions>();

        routeBuilder.MapScalarApiReference(w =>
        {
            if (!string.IsNullOrWhiteSpace(options.OpenApiPath))
                w.OpenApiRoutePattern = options.OpenApiPath;

            w.AddPreferredSecuritySchemes(["Bearer"])
             .AddHttpAuthentication("Bearer", _ => { });
        });
    }
}
