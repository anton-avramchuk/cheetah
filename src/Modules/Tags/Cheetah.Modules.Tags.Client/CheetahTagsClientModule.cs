using Cheetah.Core;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Cheetah.Modules.Tags.Client;

public sealed class TagsClientOptions
{
    /// <summary>Базовый URL Tags.Api. Обязателен.</summary>
    public string BaseUrl { get; set; } = "";

    /// <summary>Имя сервиса-владельца — проставляется во все регистрируемые типы. Обязательно.</summary>
    public string OwnerService { get; set; } = "";

    /// <summary>Тайм-аут HTTP-запросов. Default = 5 секунд.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Если true — ошибка регистрации при старте логируется, но не валит хост.
    /// Если false — исключение в IHostedService.StartAsync уронит приложение.
    /// </summary>
    public bool ContinueOnFailure { get; set; } = true;
}

internal sealed class TagsClientOptionsValidator : IValidateOptions<TagsClientOptions>
{
    public ValidateOptionsResult Validate(string? name, TagsClientOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.BaseUrl))
            return ValidateOptionsResult.Fail("Tags:Client:BaseUrl is required");
        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _))
            return ValidateOptionsResult.Fail("Tags:Client:BaseUrl must be a valid absolute URI");
        if (string.IsNullOrWhiteSpace(options.OwnerService))
            return ValidateOptionsResult.Fail("Tags:Client:OwnerService is required");
        if (options.Timeout <= TimeSpan.Zero)
            return ValidateOptionsResult.Fail("Tags:Client:Timeout must be positive");
        return ValidateOptionsResult.Success;
    }
}

/// <summary>
/// Подключает HTTP-клиент к Tags.Api + hosted-сервис, который при старте регистрирует
/// объявленные через <see cref="TaggableTypeRegistrationExtensions.AddTaggableEntityType"/> типы.
///
/// Биндит секцию <c>Tags:Client</c>; валидирует обязательные опции на старте (ValidateOnStart).
/// </summary>
[DependsOn(typeof(CoreModule), typeof(Contracts.CheetahTagsContractsModule))]
public partial class CheetahTagsClientModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        var configuration = services.GetConfiguration();

        services.AddOptions<TagsClientOptions>()
            .Bind(configuration.GetSection("Tags:Client"))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<TagsClientOptions>, TagsClientOptionsValidator>();

        services.AddHttpClient<ITagsClient, HttpTagsClient>((sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<TagsClientOptions>>().Value;
            client.BaseAddress = new Uri(opts.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = opts.Timeout;
        });

        services.AddSingleton<IHostedService, TagsRegistrationSyncService>();
    }
}
