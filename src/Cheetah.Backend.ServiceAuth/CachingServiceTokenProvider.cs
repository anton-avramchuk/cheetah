using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cheetah.Core.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Backend.ServiceAuth;

/// <summary>
/// Кэширующий поставщик сервисного токена: получает токен у Identity по client_credentials,
/// держит его в памяти и обновляет за <see cref="ServiceAuthOptions.RefreshSkew"/> до истечения.
/// Потокобезопасен; параллельные вызовы делят один запрос на обновление.
/// </summary>
[Export(LifetimeType.Singleton, typeof(IServiceTokenProvider))]
public sealed class CachingServiceTokenProvider : IServiceTokenProvider, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ServiceAuthOptions _options;
    private readonly ILogger<CachingServiceTokenProvider> _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private string? _cachedToken;
    private DateTimeOffset _refreshAt = DateTimeOffset.MinValue;

    public CachingServiceTokenProvider(
        IHttpClientFactory httpClientFactory,
        IOptions<ServiceAuthOptions> options,
        ILogger<CachingServiceTokenProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async ValueTask<string> GetTokenAsync(CancellationToken ct = default)
    {
        if (_cachedToken is not null && DateTimeOffset.UtcNow < _refreshAt)
            return _cachedToken;

        await _gate.WaitAsync(ct);
        try
        {
            // Double-check: токен мог обновить другой поток, пока мы ждали.
            if (_cachedToken is not null && DateTimeOffset.UtcNow < _refreshAt)
                return _cachedToken;

            var response = await RequestTokenAsync(ct);

            var lifetime = TimeSpan.FromSeconds(Math.Max(1, response.ExpiresIn));
            var refreshAfter = lifetime - _options.RefreshSkew;
            if (refreshAfter <= TimeSpan.Zero)
                refreshAfter = lifetime; // токен короче, чем skew — обновляем по факту истечения

            _cachedToken = response.AccessToken;
            _refreshAt = DateTimeOffset.UtcNow.Add(refreshAfter);

            _logger.LogDebug(
                "Service token for client '{ClientId}' refreshed; valid for {Seconds}s",
                _options.ClientId, response.ExpiresIn);

            return _cachedToken;
        }
        finally
        {
            _gate.Release();
        }
    }

    private async ValueTask<ServiceTokenResponse> RequestTokenAsync(CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient(CrmBackendServiceAuthModule.TokenHttpClientName);

        using var response = await client.PostAsJsonAsync(
            _options.TokenEndpoint,
            new ServiceTokenRequestBody(_options.ClientId, _options.ClientSecret),
            JsonOptions,
            ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await SafeReadAsync(response, ct);
            throw new InvalidOperationException(
                $"Service token request failed: {(int)response.StatusCode} {response.ReasonPhrase}. Body: {body}");
        }

        var token = await response.Content.ReadFromJsonAsync<ServiceTokenResponse>(JsonOptions, ct);
        if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
            throw new InvalidOperationException("Service token endpoint returned an empty token.");

        return token;
    }

    private static async ValueTask<string> SafeReadAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try { return await response.Content.ReadAsStringAsync(ct); }
        catch { return ""; }
    }

    public void Dispose() => _gate.Dispose();

    private sealed record ServiceTokenRequestBody(string ClientId, string ClientSecret);

    private sealed record ServiceTokenResponse(
        [property: JsonPropertyName("accessToken")] string AccessToken,
        [property: JsonPropertyName("tokenType")] string TokenType,
        [property: JsonPropertyName("expiresIn")] int ExpiresIn);
}
