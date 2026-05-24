using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Contracts;
using Cheetah.AspNetCore.Extensions;
using Cheetah.Backend.CQRS;
using Cheetah.Backend.Endpoints;
using Cheetah.Backend.Jwt;
using Cheetah.Backend.Rsa.Abstractions;
using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Mapster;
using Cheetah.Modules.Identity.Api.Middleware;
using Cheetah.Modules.Identity.Application;
using Cheetah.Modules.Identity.Contracts;
using Cheetah.Scalar;
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
    typeof(CrmMapsterModule),
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
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var publicKeyProvider = context.ServiceProvider.GetService<IRsaPublicKeyProvider>();
        if (publicKeyProvider is null)
            return;

        var routeBuilder = context.GetRouteBuilder();

        routeBuilder.MapGet("api/auth/public-key", () =>
            Results.Ok(new { publicKey = publicKeyProvider.PublicKeyBase64 }))
            .AllowAnonymous()
            .WithTags("Auth")
            .WithName("GetPublicKey")
            .WithOpenApi();
    }
}
