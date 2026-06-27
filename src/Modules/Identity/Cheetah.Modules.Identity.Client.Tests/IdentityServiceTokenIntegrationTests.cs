using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using Cheetah.Backend.Jwt.Options;
using Cheetah.Backend.Jwt.Services;
using Cheetah.Backend.ServiceAuth;
using Cheetah.Contracts.Responses;
using Cheetah.Modules.Identity.Contracts.Response;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shouldly;
using MsOptions = Microsoft.Extensions.Options.Options;

namespace Cheetah.Modules.Identity.Client.Tests;

/// <summary>Конкретная grid-ViewModel хоста (базовая абстрактна).</summary>
public sealed record TestUserGridVm(Guid Id, string UserName, string Email) : UserGridViewModel(Id, UserName, Email);

/// <summary>
/// Сквозной тест на сценарий «фоновая джоба Teams тянет пользователей из Identity»:
/// вызов идёт без контекста пользователя, поэтому <see cref="CheetahIdentityClientModule"/> навешивает
/// <see cref="ServiceTokenHandler"/>. Проверяем весь поток на реальных классах:
/// эмитент (Identity) подписывает RS256-токен → <see cref="CachingServiceTokenProvider"/> его забирает
/// по client_credentials → <see cref="ServiceTokenHandler"/> пишет Bearer в Authorization →
/// запрос к Identity, где токен валидируется публичным ключом (поведение валидатора в режиме JWKS).
/// </summary>
public sealed class IdentityServiceTokenIntegrationTests
{
    private const string Issuer = "cheetah";
    private const string Audience = "cheetah-clients";
    private const string IdentityBaseUrl = "https://identity.test";
    private const string TokenEndpoint = "https://identity.test/api/auth/service-token";

    [Fact]
    public async Task BackgroundSync_SignsRs256ServiceToken_ForwardsItToIdentity_AndIdentityValidatesByPublicKey()
    {
        // --- Эмитент Identity: реальная RS256-подпись (тот же код, что в проде) ---
        using var rsa = RSA.Create(2048);
        var issuerKeyProvider = new RsaJwtSigningKeyProvider(MsOptions.Create(new JwtOptions
        {
            SigningAlgorithm = JwtSigningAlgorithms.Rs256,
            PrivateKeyPem = rsa.ExportPkcs8PrivateKeyPem(),
            Issuer = Issuer,
            Audience = Audience,
        }));
        var generator = new JwtTokenGenerator(
            MsOptions.Create(new JwtOptions { Issuer = Issuer, Audience = Audience }),
            issuerKeyProvider);

        var issuedToken = generator.GenerateServiceToken("svc-teams", new[] { "ServiceAccount" }).Token;

        // Эндпоинт выпуска токена: отдаёт подписанный RS256-токен по client_credentials.
        var tokenEndpoint = new StubHandler(req =>
        {
            req.RequestUri!.ToString().ShouldBe(TokenEndpoint);
            return JsonResponse(new { accessToken = issuedToken, tokenType = "Bearer", expiresIn = 600 });
        });

        // Сервис-валидатор (Identity API /api/users): проверяет подпись публичным ключом (как по JWKS).
        var identityApi = new StubHandler(req =>
        {
            var auth = req.Headers.Authorization;
            if (auth is null || auth.Scheme != "Bearer")
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);

            if (!TryValidate(auth.Parameter!, issuerKeyProvider.GetValidationKey()))
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);

            var users = new GridResult<TestUserGridVm>(
                new[] { new TestUserGridVm(Guid.NewGuid(), "alice", "alice@cheetah.io") }, total: 1);
            return JsonResponse(users);
        });

        var sp = BuildConsumer(tokenEndpoint, identityApi);

        // --- Потребитель (как фоновый синк Teams): дергаем S2S-клиент Identity ---
        var client = sp.GetRequiredService<IIdentityUsersClient>();
        var result = await client.GetUsersAsync();

        // Identity вернул пользователей → значит токен дошёл и прошёл валидацию подписи.
        result.Count.ShouldBe(1);
        result[0].UserName.ShouldBe("alice");

        // Запрос к Identity нёс именно выпущенный RS256-токен.
        var forwarded = identityApi.LastRequest!.Headers.Authorization!;
        forwarded.Scheme.ShouldBe("Bearer");
        forwarded.Parameter.ShouldBe(issuedToken);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(forwarded.Parameter);
        jwt.Header.Alg.ShouldBe(SecurityAlgorithms.RsaSha256);
        jwt.Header.Kid.ShouldBe(issuerKeyProvider.GetPublicWebKeys()[0].Kid);
        jwt.Claims.ShouldContain(c => c.Type == "token_type" && c.Value == "service");
    }

    [Fact]
    public async Task TamperedToken_IsRejectedByIdentity_AndYieldsNoUsers()
    {
        using var issuerRsa = RSA.Create(2048);
        var issuerKeyProvider = new RsaJwtSigningKeyProvider(MsOptions.Create(new JwtOptions
        {
            SigningAlgorithm = JwtSigningAlgorithms.Rs256,
            PrivateKeyPem = issuerRsa.ExportPkcs8PrivateKeyPem(),
            Issuer = Issuer,
            Audience = Audience,
        }));

        // Токен подписан ЧУЖИМ ключом — публичный ключ эмитента его не подтвердит.
        using var foreignRsa = RSA.Create(2048);
        var foreignKeyProvider = new RsaJwtSigningKeyProvider(MsOptions.Create(new JwtOptions
        {
            SigningAlgorithm = JwtSigningAlgorithms.Rs256,
            PrivateKeyPem = foreignRsa.ExportPkcs8PrivateKeyPem(),
            Issuer = Issuer,
            Audience = Audience,
        }));
        var foreignGenerator = new JwtTokenGenerator(
            MsOptions.Create(new JwtOptions { Issuer = Issuer, Audience = Audience }),
            foreignKeyProvider);
        var foreignToken = foreignGenerator.GenerateServiceToken("svc-teams", new[] { "ServiceAccount" }).Token;

        var tokenEndpoint = new StubHandler(_ =>
            JsonResponse(new { accessToken = foreignToken, tokenType = "Bearer", expiresIn = 600 }));

        var identityApi = new StubHandler(req =>
        {
            var auth = req.Headers.Authorization;
            if (auth is null || !TryValidate(auth.Parameter!, issuerKeyProvider.GetValidationKey()))
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            return JsonResponse(new GridResult<TestUserGridVm>(Array.Empty<TestUserGridVm>(), 0));
        });

        var sp = BuildConsumer(tokenEndpoint, identityApi);
        var client = sp.GetRequiredService<IIdentityUsersClient>();

        // 401 → HttpIdentityUsersClient.GetFromJsonAsync бросит; синк бы залогировал и повторил.
        await Should.ThrowAsync<HttpRequestException>(async () => await client.GetUsersAsync());
        identityApi.LastRequest!.Headers.Authorization!.Parameter.ShouldBe(foreignToken);
    }

    /// <summary>
    /// Собирает DI-граф потребителя точно так, как это делают модули в проде:
    /// ServiceAuth (<see cref="CrmBackendServiceAuthModule"/>) + Identity-клиент
    /// (<see cref="CheetahIdentityClientModule"/>) с навешенным <see cref="ServiceTokenHandler"/>.
    /// Сеть подменяется стабами через ConfigurePrimaryHttpMessageHandler.
    /// </summary>
    private static ServiceProvider BuildConsumer(StubHandler tokenEndpoint, StubHandler identityApi)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Identity:Client:BaseUrl"] = IdentityBaseUrl,
            ["ServiceAuth:TokenEndpoint"] = TokenEndpoint,
            ["ServiceAuth:ClientId"] = "svc-teams",
            ["ServiceAuth:ClientSecret"] = "teams-secret",
        }).Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(config);

        // == зеркало CrmBackendServiceAuthModule ==
        services.AddOptions<ServiceAuthOptions>().Bind(config.GetSection(ServiceAuthOptions.SectionName));
        services.AddSingleton<IServiceTokenProvider, CachingServiceTokenProvider>();
        services.AddTransient<ServiceTokenHandler>();
        services.AddHttpClient(CrmBackendServiceAuthModule.TokenHttpClientName)
            .ConfigurePrimaryHttpMessageHandler(() => tokenEndpoint);

        // == зеркало CheetahIdentityClientModule (тестируемая проводка) ==
        services.AddOptions<IdentityClientOptions>().Bind(config.GetSection("Identity:Client"));
        services.AddHttpClient<IIdentityUsersClient, HttpIdentityUsersClient>((sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<IdentityClientOptions>>().Value;
                client.BaseAddress = new Uri(opts.BaseUrl.TrimEnd('/') + "/");
                client.Timeout = opts.Timeout;
            })
            .AddHttpMessageHandler<ServiceTokenHandler>()
            .ConfigurePrimaryHttpMessageHandler(() => identityApi);

        return services.BuildServiceProvider();
    }

    private static bool TryValidate(string token, SecurityKey publicKey)
    {
        try
        {
            new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
            {
                ValidIssuer = Issuer,
                ValidAudience = Audience,
                IssuerSigningKey = publicKey,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
            }, out _);
            return true;
        }
        catch (SecurityTokenException)
        {
            return false;
        }
    }

    private static HttpResponseMessage JsonResponse(object payload)
        => new(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(payload, options: new JsonSerializerOptions(JsonSerializerDefaults.Web)),
        };

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(respond(request));
        }
    }
}
