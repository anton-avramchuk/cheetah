using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Contracts;
using Cheetah.AspNetCore.Extensions;
using Cheetah.Backend.CQRS;
using Cheetah.Backend.Endpoints;
using Cheetah.Backend.Jwt;
using Cheetah.Backend.Jwt.Options;
using Cheetah.Backend.Jwt.Services;
using Cheetah.Backend.Rsa.Abstractions;
using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Identity.Api.Middleware;
using Cheetah.Modules.Identity.Api.ServiceClients;
using Cheetah.Modules.Identity.Application;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Contracts;
using Cheetah.Modules.Identity.Contracts.Requests;
using Cheetah.Modules.Identity.Contracts.Response;
using Cheetah.Scalar;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

// optional dependency — see comment on [DependsOn]

namespace Cheetah.Modules.Identity.Api;

// CrmBackendRsaModule is intentionally NOT listed here — RSA is optional.
// Add it in your host's module dependencies to enable RSA password decryption
// and expose the GET api/auth/public-key endpoint automatically.
[DependsOn(
    typeof(CoreModule),
    typeof(CrmAspNetCoreModule),
    typeof(CrmAspNetCoreContractsModule),
    typeof(ScalarModule),
    typeof(CrmBackendCQRSModule),
    typeof(CrmBackendEndpointsModule),
    typeof(CrmBackendJwtModule),
    typeof(CheetahIdentityApplicationModule),
    typeof(CheetahIdentityContractsModule)
)]
public partial class CheetahIdentityApiModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
        context.Services.AddExceptionHandler<InvalidCredentialsExceptionHandler>();

        context.Services
            .AddOptions<ServiceClientsOptions>()
            .BindConfiguration(ServiceClientsOptions.SectionName);
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var routeBuilder = context.GetRouteBuilder();

        // Сервисный токен (machine-to-machine), схема client_credentials.
        routeBuilder.MapPost("api/auth/service-token", async (
            [FromBody] ServiceTokenRequest request,
            [FromServices] IDispatcher dispatcher,
            CancellationToken ct) =>
        {
            var command = new IssueServiceTokenCommand(request.ClientId, request.ClientSecret);
            var result = await dispatcher.SendAsync<IssueServiceTokenCommand, TokenResult>(command, ct);
            return Results.Ok(new TokenViewModel(result.Token, "Bearer", result.ExpiresInSeconds));
        })
            .AllowAnonymous()
            .WithTags("Auth")
            .WithName("IssueServiceToken");

        MapJwksEndpoints(context, routeBuilder);

        var publicKeyProvider = context.ServiceProvider.GetService<IRsaPublicKeyProvider>();
        if (publicKeyProvider is null)
            return;

        routeBuilder.MapGet("api/auth/public-key", () =>
            Results.Ok(new { publicKey = publicKeyProvider.PublicKeyBase64 }))
            .AllowAnonymous()
            .WithTags("Auth")
            .WithName("GetPublicKey");
    }

    // JWKS + OIDC discovery — публикуются только при асимметричной подписи (RS256),
    // чтобы сервисы-валидаторы брали публичный ключ по сети (Jwt:MetadataAddress).
    private static void MapJwksEndpoints(ApplicationInitializationContext context, IEndpointRouteBuilder routeBuilder)
    {
        var keyProvider = context.ServiceProvider.GetRequiredService<IJwtSigningKeyProvider>();
        if (!keyProvider.IsAsymmetric)
            return;

        var issuer = context.ServiceProvider.GetRequiredService<IOptions<JwtOptions>>().Value.Issuer;

        routeBuilder.MapGet("/.well-known/jwks.json", ([FromServices] IJwtSigningKeyProvider keys) =>
        {
            var jwks = keys.GetPublicWebKeys().Select(k => new
            {
                kty = k.Kty,
                use = k.Use,
                kid = k.Kid,
                alg = k.Alg,
                n = k.N,
                e = k.E,
            });
            return Results.Json(new { keys = jwks });
        })
            .AllowAnonymous()
            .WithTags("Auth")
            .WithName("Jwks");

        routeBuilder.MapGet("/.well-known/openid-configuration", (HttpRequest request) =>
        {
            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
            return Results.Json(new
            {
                issuer,
                jwks_uri = $"{baseUrl}/.well-known/jwks.json",
                id_token_signing_alg_values_supported = new[] { JwtSigningAlgorithms.Rs256 },
                response_types_supported = new[] { "token" },
                subject_types_supported = new[] { "public" },
            });
        })
            .AllowAnonymous()
            .WithTags("Auth")
            .WithName("OpenIdConfiguration");
    }
}
