using Cheetah.Core;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cheetah.Backend.ServiceAuth;

/// <summary>
/// Подключает machine-to-machine аутентификацию для исходящих HTTP-вызовов:
/// <see cref="IServiceTokenProvider"/> (кэш + обновление токена) и
/// <see cref="ServiceTokenHandler"/> (подстановка Bearer). Биндит секцию <c>ServiceAuth</c>.
/// </summary>
/// <remarks>
/// Чтобы клиент слал сервисный токен, добавьте обработчик к его <see cref="HttpClient"/>:
/// <c>.AddHttpMessageHandler&lt;ServiceTokenHandler&gt;()</c> и сделайте модуль клиента
/// зависимым от этого модуля.
/// </remarks>
[DependsOn(typeof(CoreModule))]
public partial class CrmBackendServiceAuthModule : CrmModule
{
    /// <summary>Имя <see cref="HttpClient"/> для запроса токена у Identity (без Bearer-обработчика).</summary>
    public const string TokenHttpClientName = "Cheetah.ServiceAuth.Token";

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        var services = context.Services;
        var configuration = services.GetConfiguration();

        services.AddOptions<ServiceAuthOptions>()
            .Bind(configuration.GetSection(ServiceAuthOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<ServiceAuthOptions>, ServiceAuthOptionsValidator>();

        // Отдельный клиент для запроса токена — БЕЗ ServiceTokenHandler, чтобы не зациклиться.
        services.AddHttpClient(TokenHttpClientName, (sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<ServiceAuthOptions>>().Value;
            client.Timeout = opts.Timeout;
        });
    }
}

internal sealed class ServiceAuthOptionsValidator : IValidateOptions<ServiceAuthOptions>
{
    public ValidateOptionsResult Validate(string? name, ServiceAuthOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.TokenEndpoint))
            return ValidateOptionsResult.Fail("ServiceAuth:TokenEndpoint is required");
        if (!Uri.TryCreate(options.TokenEndpoint, UriKind.Absolute, out _))
            return ValidateOptionsResult.Fail("ServiceAuth:TokenEndpoint must be a valid absolute URI");
        if (string.IsNullOrWhiteSpace(options.ClientId))
            return ValidateOptionsResult.Fail("ServiceAuth:ClientId is required");
        if (string.IsNullOrWhiteSpace(options.ClientSecret))
            return ValidateOptionsResult.Fail("ServiceAuth:ClientSecret is required");
        if (options.Timeout <= TimeSpan.Zero)
            return ValidateOptionsResult.Fail("ServiceAuth:Timeout must be positive");
        return ValidateOptionsResult.Success;
    }
}
