using Cheetah.Backend.ServiceAuth;
using Cheetah.Core;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Identity.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cheetah.Modules.Identity.Client;

public sealed class IdentityClientOptions
{
    /// <summary>Базовый URL Identity API. Обязателен.</summary>
    public string BaseUrl { get; set; } = "";

    /// <summary>Тайм-аут HTTP-запросов. Default = 10 секунд.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
}

internal sealed class IdentityClientOptionsValidator : IValidateOptions<IdentityClientOptions>
{
    public ValidateOptionsResult Validate(string? name, IdentityClientOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.BaseUrl))
            return ValidateOptionsResult.Fail("Identity:Client:BaseUrl is required");
        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _))
            return ValidateOptionsResult.Fail("Identity:Client:BaseUrl must be a valid absolute URI");
        if (options.Timeout <= TimeSpan.Zero)
            return ValidateOptionsResult.Fail("Identity:Client:Timeout must be positive");
        return ValidateOptionsResult.Success;
    }
}

/// <summary>
/// Подключает HTTP-клиент к Identity API (<see cref="IIdentityUsersClient"/>).
/// Биндит секцию <c>Identity:Client</c>; валидирует обязательные опции на старте (ValidateOnStart).
/// <para>
/// Это server-to-server клиент: вызовы идут без контекста пользователя (например, фоновый синк
/// участников в Teams/Tags). Поэтому на него навешивается <see cref="ServiceTokenHandler"/> —
/// каждый исходящий запрос несёт сервисный токен (machine-to-machine, схема client_credentials)
/// в заголовке <c>Authorization: Bearer</c>. Хост обязан настроить секцию <c>ServiceAuth</c>
/// (см. <see cref="CrmBackendServiceAuthModule"/>).
/// </para>
/// </summary>
[DependsOn(typeof(CoreModule), typeof(CheetahIdentityContractsModule), typeof(CrmBackendServiceAuthModule))]
public partial class CheetahIdentityClientModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        var configuration = services.GetConfiguration();

        services.AddOptions<IdentityClientOptions>()
            .Bind(configuration.GetSection("Identity:Client"))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<IdentityClientOptions>, IdentityClientOptionsValidator>();

        services.AddHttpClient<IIdentityUsersClient, HttpIdentityUsersClient>((sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<IdentityClientOptions>>().Value;
            client.BaseAddress = new Uri(opts.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = opts.Timeout;
        })
        .AddHttpMessageHandler<ServiceTokenHandler>(); // фоновые S2S-вызовы → сервисный токен в Authorization
    }
}
