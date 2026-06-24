using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Extensions;
using Cheetah.Backend.Jwt.Options;
using Cheetah.Backend.Jwt.Services;
using Cheetah.Core;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Cheetah.Backend.Jwt;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmAspNetCoreModule))]
public partial class CrmBackendJwtModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        context.Services
            .AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        context.Services.AddSingleton<IValidateOptions<JwtOptions>, JwtOptionsValidator>();

        RegisterSigningKeyProvider(context.Services);

        context.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer();

        context.Services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>, IJwtSigningKeyProvider>((bearerOptions, jwtOptions, keyProvider) =>
            {
                var opts = jwtOptions.Value;

                bearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = opts.Issuer,
                    ValidAudience = opts.Audience,
                    ClockSkew = TimeSpan.Zero,
                };

                if (!string.IsNullOrWhiteSpace(opts.MetadataAddress))
                {
                    // Микросервисный режим: ключи берутся из JWKS эмитента (подбор по kid).
                    bearerOptions.MetadataAddress = opts.MetadataAddress;
                    bearerOptions.RequireHttpsMetadata = opts.RequireHttpsMetadata;
                }
                else
                {
                    // Локальный режим: валидация ключом этого процесса (HMAC или публичный RSA).
                    bearerOptions.TokenValidationParameters.IssuerSigningKey = keyProvider.GetValidationKey();
                }
            });

        context.Services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });
    }

    private static void RegisterSigningKeyProvider(IServiceCollection services)
    {
        var algorithm = services.GetConfiguration()[$"{JwtOptions.SectionName}:{nameof(JwtOptions.SigningAlgorithm)}"];

        if (string.Equals(algorithm, JwtSigningAlgorithms.Rs256, StringComparison.OrdinalIgnoreCase))
            services.AddSingleton<IJwtSigningKeyProvider, RsaJwtSigningKeyProvider>();
        else
            services.AddSingleton<IJwtSigningKeyProvider, HmacJwtSigningKeyProvider>();
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        app.UseAuthentication();
        app.UseAuthorization();
    }
}
