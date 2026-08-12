using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Extensions;
using Cheetah.Core;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Cheetah.OpenApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace Cheetah.Scalar;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmAspNetCoreModule), typeof(OpenApiModule))]
public partial class ScalarModule : CrmModule
{
    /// <summary>
    /// Значение <c>Scalar:SecurityScheme</c>, отключающее схему авторизации целиком. Нужно сервису,
    /// который пускает не по токену: BFF держит cookie-сессию, и объявленный Bearer означал бы в
    /// спеке и в UI поле для токена, которого у него не бывает.
    /// </summary>
    public const string NoSecurityScheme = "None";

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        var configuration = context.Services.GetConfiguration();
        var documentName = configuration["OpenApi:DocumentName"] ?? Cheetah.OpenApi.OpenApiConstants.DefaultDocumentName;

        if (IsBearerDisabled(configuration["Scalar:SecurityScheme"]))
            return;

        context.Services.AddOpenApi(documentName, options =>
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
        var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
        var documentName = configuration["OpenApi:DocumentName"] ?? Cheetah.OpenApi.OpenApiConstants.DefaultDocumentName;
        var bearerDisabled = IsBearerDisabled(configuration["Scalar:SecurityScheme"]);

        routeBuilder.MapScalarApiReference(w =>
        {
            w.OpenApiRoutePattern = !string.IsNullOrWhiteSpace(options.OpenApiPath)
                ? options.OpenApiPath
                : $"/openapi/{documentName}.json";

            if (bearerDisabled)
                return;

            w.AddPreferredSecuritySchemes(["Bearer"])
             .AddHttpAuthentication("Bearer", _ => { });
        }).AllowAnonymous();
    }

    private static bool IsBearerDisabled(string? scheme)
        => string.Equals(scheme, NoSecurityScheme, StringComparison.OrdinalIgnoreCase);
}
