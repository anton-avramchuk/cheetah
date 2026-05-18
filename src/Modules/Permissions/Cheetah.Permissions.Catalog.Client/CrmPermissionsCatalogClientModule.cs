using Cheetah.Core;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Cheetah.Permissions.Catalog.Client;

public class PermissionsCatalogClientOptions
{
    /// <summary>Базовый URL Permissions.Catalog.Api. Обязателен.</summary>
    public string BaseUrl { get; set; } = "";

    /// <summary>Тайм-аут HTTP-запросов. Default = 5 секунд.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Если true — ошибки sync логируются как warning, но не останавливают приложение.
    /// Если false — необработанное исключение в IHostedService.StartAsync уронит хост целиком
    /// (нужно, когда работа без registered permissions — недопустима, например в strict-проде).
    /// </summary>
    public bool ContinueOnFailure { get; set; } = true;
}

internal sealed class PermissionsCatalogClientOptionsValidator
    : IValidateOptions<PermissionsCatalogClientOptions>
{
    public ValidateOptionsResult Validate(string? name, PermissionsCatalogClientOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.BaseUrl))
            return ValidateOptionsResult.Fail("Permissions:CatalogClient:BaseUrl is required");
        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _))
            return ValidateOptionsResult.Fail("Permissions:CatalogClient:BaseUrl must be a valid absolute URI");
        if (options.Timeout <= TimeSpan.Zero)
            return ValidateOptionsResult.Fail("Permissions:CatalogClient:Timeout must be positive");
        return ValidateOptionsResult.Success;
    }
}

/// <summary>
/// Подключает HTTP-клиент к Permissions.Catalog.Api + hosted-сервис автоматической
/// регистрации permissions микросервиса при старте.
///
/// Биндит секцию <c>Permissions:CatalogClient</c>. Валидирует обязательные опции при старте
/// через ValidateOnStart() — приложение не поднимется с пустым/невалидным BaseUrl.
/// </summary>
[DependsOn(typeof(CoreModule), typeof(CrmPermissionsModule))]
public partial class CrmPermissionsCatalogClientModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        var configuration = services.GetConfiguration();

        services.AddOptions<PermissionsCatalogClientOptions>()
            .Bind(configuration.GetSection("Permissions:CatalogClient"))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<PermissionsCatalogClientOptions>,
            PermissionsCatalogClientOptionsValidator>();

        services.AddHttpClient<IPermissionsCatalogClient, HttpPermissionsCatalogClient>((sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<PermissionsCatalogClientOptions>>().Value;
            client.BaseAddress = new Uri(opts.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = opts.Timeout;
        });

        services.AddSingleton<IHostedService, RemoteRegistrySyncService>();
    }
}
