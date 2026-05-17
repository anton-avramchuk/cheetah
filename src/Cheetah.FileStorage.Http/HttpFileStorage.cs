using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Cheetah.Core.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.FileStorage.Http;

/// <summary>
/// Клиент удалённого file-storage сервиса. Все операции — HTTP-запросы к BaseUrl.
/// Используется когда хранилище вынесено в отдельный микросервис (центральный
/// storage-сервис, который сам решает — Local, S3 или что-то ещё).
/// </summary>
[Export(LifetimeType.Singleton, typeof(IFileStorage))]
public sealed class HttpFileStorage : IFileStorage
{
    public const string HttpClientName = "Cheetah.FileStorage.Http";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly HttpFileStorageOptions _options;
    private readonly ILogger<HttpFileStorage> _logger;

    public HttpFileStorage(IHttpClientFactory httpClientFactory, IOptions<HttpFileStorageOptions> options, ILogger<HttpFileStorage> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_options.BaseUrl))
            throw new InvalidOperationException("HttpFileStorageOptions.BaseUrl is required");
    }

    public async ValueTask SaveAsync(string key, Stream content, string contentType,
        IReadOnlyDictionary<string, string>? userMetadata = null,
        CancellationToken cancellationToken = default)
    {
        StorageKey.Validate(key);

        var client = CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Put, BuildUrl(key))
        {
            Content = new StreamContent(content)
        };
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);

        if (userMetadata is not null)
        {
            foreach (var (k, v) in userMetadata)
                request.Headers.Add($"X-Meta-{k}", v);
        }

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        await EnsureSuccessAsync(response, key).ConfigureAwait(false);
        _logger.LogDebug("Saved remote file '{Key}'", key);
    }

    public async ValueTask<Stream> OpenReadAsync(string key, CancellationToken cancellationToken = default)
    {
        StorageKey.Validate(key);
        var client = CreateClient();
        var response = await client.GetAsync(BuildUrl(key), HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            response.Dispose();
            throw new FileStorageNotFoundException(key);
        }
        await EnsureSuccessAsync(response, key).ConfigureAwait(false);

        // Поток держит ownership и над response — оборачиваем чтобы Dispose закрыл оба.
        var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return new OwningStream(stream, response);
    }

    public async ValueTask DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        StorageKey.Validate(key);
        var client = CreateClient();
        using var response = await client.DeleteAsync(BuildUrl(key), cancellationToken).ConfigureAwait(false);

        // 404 при DELETE — норма (идемпотентность).
        if (response.StatusCode == HttpStatusCode.NotFound) return;
        await EnsureSuccessAsync(response, key).ConfigureAwait(false);
    }

    public async ValueTask<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        StorageKey.Validate(key);
        var client = CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Head, BuildUrl(key));
        using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.NotFound) return false;
        await EnsureSuccessAsync(response, key).ConfigureAwait(false);
        return true;
    }

    public async ValueTask<FileMetadata?> GetMetadataAsync(string key, CancellationToken cancellationToken = default)
    {
        StorageKey.Validate(key);
        var client = CreateClient();
        var response = await client.GetAsync($"{BuildUrl(key)}/metadata", cancellationToken).ConfigureAwait(false);
        try
        {
            if (response.StatusCode == HttpStatusCode.NotFound) return null;
            await EnsureSuccessAsync(response, key).ConfigureAwait(false);

            var dto = await response.Content.ReadFromJsonAsync<MetadataDto>(cancellationToken).ConfigureAwait(false);
            if (dto is null) return null;

            return new FileMetadata(dto.size, dto.contentType, dto.lastModified, dto.userMetadata);
        }
        finally
        {
            response.Dispose();
        }
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient(HttpClientName);
        client.Timeout = _options.Timeout;
        if (!string.IsNullOrEmpty(_options.ApiKey))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        return client;
    }

    private string BuildUrl(string key) => $"{_options.BaseUrl.TrimEnd('/')}/files/{key}";

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, string key)
    {
        if (response.IsSuccessStatusCode) return;
        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        throw new FileStorageException(
            $"Remote storage returned {(int)response.StatusCode} {response.ReasonPhrase} for '{key}': {body}");
    }

    private sealed record MetadataDto(long size, string contentType, DateTimeOffset lastModified, Dictionary<string, string>? userMetadata);

    /// <summary>
    /// Stream-обёртка, которая при Dispose закрывает и сам поток, и HttpResponseMessage,
    /// чтобы клиент мог использовать `await using var s = ...` без утечки соединения.
    /// </summary>
    private sealed class OwningStream : Stream
    {
        private readonly Stream _inner;
        private readonly HttpResponseMessage _response;

        public OwningStream(Stream inner, HttpResponseMessage response)
        {
            _inner = inner;
            _response = response;
        }

        public override bool CanRead => _inner.CanRead;
        public override bool CanSeek => _inner.CanSeek;
        public override bool CanWrite => _inner.CanWrite;
        public override long Length => _inner.Length;
        public override long Position { get => _inner.Position; set => _inner.Position = value; }
        public override void Flush() => _inner.Flush();
        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken ct)
            => _inner.ReadAsync(buffer, offset, count, ct);
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default)
            => _inner.ReadAsync(buffer, ct);
        public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
        public override void SetLength(long value) => _inner.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _inner.Dispose();
                _response.Dispose();
            }
            base.Dispose(disposing);
        }

        public override async ValueTask DisposeAsync()
        {
            await _inner.DisposeAsync().ConfigureAwait(false);
            _response.Dispose();
            await base.DisposeAsync().ConfigureAwait(false);
        }
    }
}
