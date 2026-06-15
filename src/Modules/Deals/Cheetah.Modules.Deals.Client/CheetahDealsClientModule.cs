using Cheetah.Core;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cheetah.Modules.Deals.Client;

public sealed class DealsClientOptions
{
    /// <summary>Базовый URL Deals.Api. Обязателен.</summary>
    public string BaseUrl { get; set; } = "";

    /// <summary>Тайм-аут HTTP-запросов. Default = 5 секунд.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);
}

internal sealed class DealsClientOptionsValidator : IValidateOptions<DealsClientOptions>
{
    public ValidateOptionsResult Validate(string? name, DealsClientOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.BaseUrl))
            return ValidateOptionsResult.Fail("Deals:Client:BaseUrl is required");
        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _))
            return ValidateOptionsResult.Fail("Deals:Client:BaseUrl must be a valid absolute URI");
        if (options.Timeout <= TimeSpan.Zero)
            return ValidateOptionsResult.Fail("Deals:Client:Timeout must be positive");
        return ValidateOptionsResult.Success;
    }
}

/// <summary>
/// Подключает HTTP-клиент к Deals.Api. Биндит секцию <c>Deals:Client</c>; валидирует обязательные
/// опции на старте (ValidateOnStart).
/// </summary>
[DependsOn(typeof(CoreModule), typeof(Contracts.CheetahDealsContractsModule))]
public partial class CheetahDealsClientModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        var configuration = services.GetConfiguration();

        services.AddOptions<DealsClientOptions>()
            .Bind(configuration.GetSection("Deals:Client"))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<DealsClientOptions>, DealsClientOptionsValidator>();

        services.AddHttpClient<IDealsClient, HttpDealsClient>((sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<DealsClientOptions>>().Value;
            client.BaseAddress = new Uri(opts.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = opts.Timeout;
        });
    }
}
