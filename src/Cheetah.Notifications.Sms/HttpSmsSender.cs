using System.Net.Http.Headers;
using System.Net.Http.Json;
using Cheetah.Core.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Notifications.Sms;

/// <summary>
/// Провайдер-агностичный SMS-sender: POST {"to","body"} на SmsOptions.ProviderUrl.
/// Конкретные интеграции (Twilio, SMSC, ...) реализуются как замена этого sender'a
/// либо как proxy-сервис, который принимает наш формат и транслирует в нужный API.
/// </summary>
[Export(LifetimeType.Scoped, typeof(INotificationSender<SmsMessage>))]
public sealed class HttpSmsSender : INotificationSender<SmsMessage>
{
    public const string HttpClientName = "Cheetah.Notifications.Sms";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SmsOptions _options;
    private readonly ILogger<HttpSmsSender> _logger;

    public HttpSmsSender(IHttpClientFactory httpClientFactory, IOptions<SmsOptions> options, ILogger<HttpSmsSender> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async ValueTask SendAsync(SmsMessage message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ProviderUrl))
            throw new InvalidOperationException("SmsOptions.ProviderUrl не задан. Настройте Notifications:Sms:ProviderUrl.");

        var client = _httpClientFactory.CreateClient(HttpClientName);
        client.Timeout = _options.Timeout;

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.ProviderUrl)
        {
            Content = JsonContent.Create(new { to = message.To, body = message.Body })
        };
        if (!string.IsNullOrEmpty(_options.ApiKey))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        }

        _logger.LogDebug("Sending SMS to {To}", message.To);

        using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }
}
