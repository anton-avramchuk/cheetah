using Cheetah.Core;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cheetah.Modules.Booking.Client;

public sealed class BookingClientOptions
{
    /// <summary>Базовый URL Booking.Api. Обязателен.</summary>
    public string BaseUrl { get; set; } = "";

    /// <summary>Тайм-аут HTTP-запросов. Default = 5 секунд.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);
}

internal sealed class BookingClientOptionsValidator : IValidateOptions<BookingClientOptions>
{
    public ValidateOptionsResult Validate(string? name, BookingClientOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.BaseUrl))
            return ValidateOptionsResult.Fail("Booking:Client:BaseUrl is required");
        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _))
            return ValidateOptionsResult.Fail("Booking:Client:BaseUrl must be a valid absolute URI");
        if (options.Timeout <= TimeSpan.Zero)
            return ValidateOptionsResult.Fail("Booking:Client:Timeout must be positive");
        return ValidateOptionsResult.Success;
    }
}

/// <summary>
/// Подключает HTTP-клиент к Booking.Api. Биндит секцию <c>Booking:Client</c>; валидирует обязательные
/// опции на старте (ValidateOnStart).
/// </summary>
[DependsOn(typeof(CoreModule), typeof(Contracts.CheetahBookingContractsModule))]
public partial class CheetahBookingClientModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        var configuration = services.GetConfiguration();

        services.AddOptions<BookingClientOptions>()
            .Bind(configuration.GetSection("Booking:Client"))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<BookingClientOptions>, BookingClientOptionsValidator>();

        services.AddHttpClient<IBookingClient, HttpBookingClient>((sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<BookingClientOptions>>().Value;
            client.BaseAddress = new Uri(opts.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = opts.Timeout;
        });
    }
}
