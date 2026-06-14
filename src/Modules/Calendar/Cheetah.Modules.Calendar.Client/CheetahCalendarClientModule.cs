using Cheetah.Core;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Cheetah.Modules.Calendar.Client;

public sealed class CalendarClientOptions
{
    /// <summary>Базовый URL Calendar.Api. Обязателен.</summary>
    public string BaseUrl { get; set; } = "";

    /// <summary>Имя сервиса-владельца — проставляется во все регистрируемые типы. Обязательно.</summary>
    public string OwnerService { get; set; } = "";

    /// <summary>Тайм-аут HTTP-запросов. Default = 5 секунд.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>Если true — ошибка регистрации при старте логируется, но не валит хост.</summary>
    public bool ContinueOnFailure { get; set; } = true;
}

internal sealed class CalendarClientOptionsValidator : IValidateOptions<CalendarClientOptions>
{
    public ValidateOptionsResult Validate(string? name, CalendarClientOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.BaseUrl))
            return ValidateOptionsResult.Fail("Calendar:Client:BaseUrl is required");
        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _))
            return ValidateOptionsResult.Fail("Calendar:Client:BaseUrl must be a valid absolute URI");
        if (string.IsNullOrWhiteSpace(options.OwnerService))
            return ValidateOptionsResult.Fail("Calendar:Client:OwnerService is required");
        if (options.Timeout <= TimeSpan.Zero)
            return ValidateOptionsResult.Fail("Calendar:Client:Timeout must be positive");
        return ValidateOptionsResult.Success;
    }
}

/// <summary>
/// Подключает HTTP-клиент к Calendar.Api + hosted-сервис, который при старте регистрирует
/// объявленные через <see cref="CalendarableTypeRegistrationExtensions.AddCalendarableEntityType"/> типы.
/// Биндит секцию <c>Calendar:Client</c>; валидирует обязательные опции на старте (ValidateOnStart).
/// </summary>
[DependsOn(typeof(CoreModule), typeof(Contracts.CheetahCalendarContractsModule))]
public partial class CheetahCalendarClientModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        var configuration = services.GetConfiguration();

        services.AddOptions<CalendarClientOptions>()
            .Bind(configuration.GetSection("Calendar:Client"))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<CalendarClientOptions>, CalendarClientOptionsValidator>();

        services.AddHttpClient<ICalendarClient, HttpCalendarClient>((sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<CalendarClientOptions>>().Value;
            client.BaseAddress = new Uri(opts.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = opts.Timeout;
        });

        services.AddSingleton<IHostedService, CalendarRegistrationSyncService>();
    }
}
