using Amazon.S3;
using Amazon.S3.Model;
using Cheetah.Core.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.FileStorage.S3;

/// <summary>
/// S3-реализация IFileStorage. Совместима с любым S3-API: AWS S3, MinIO, Yandex Object Storage,
/// Backblaze B2, R2. Конкретный endpoint и стиль адресации задаются S3FileStorageOptions.
/// </summary>
[Export(LifetimeType.Singleton, typeof(IFileStorage))]
public sealed class S3FileStorage : IFileStorage
{
    private readonly IAmazonS3 _client;
    private readonly S3FileStorageOptions _options;
    private readonly ILogger<S3FileStorage> _logger;

    public S3FileStorage(IAmazonS3 client, IOptions<S3FileStorageOptions> options, ILogger<S3FileStorage> logger)
    {
        _client = client;
        _options = options.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_options.BucketName))
            throw new InvalidOperationException("S3FileStorageOptions.BucketName is required");
    }

    public async ValueTask SaveAsync(string key, Stream content, string contentType,
        IReadOnlyDictionary<string, string>? userMetadata = null,
        CancellationToken cancellationToken = default)
    {
        StorageKey.Validate(key);

        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            InputStream = content,
            ContentType = contentType,
            AutoCloseStream = false
        };
        if (userMetadata is not null)
        {
            foreach (var (k, v) in userMetadata)
                request.Metadata[k] = v;
        }

        try
        {
            await _client.PutObjectAsync(request, cancellationToken).ConfigureAwait(false);
            _logger.LogDebug("Saved S3 object {Bucket}/{Key}", _options.BucketName, key);
        }
        catch (AmazonS3Exception ex)
        {
            throw new FileStorageException($"Failed to save '{key}' to S3", ex);
        }
    }

    public async ValueTask<Stream> OpenReadAsync(string key, CancellationToken cancellationToken = default)
    {
        StorageKey.Validate(key);
        try
        {
            var response = await _client.GetObjectAsync(_options.BucketName, key, cancellationToken).ConfigureAwait(false);
            return response.ResponseStream;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new FileStorageNotFoundException(key);
        }
        catch (AmazonS3Exception ex)
        {
            throw new FileStorageException($"Failed to read '{key}' from S3", ex);
        }
    }

    public async ValueTask DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        StorageKey.Validate(key);
        try
        {
            await _client.DeleteObjectAsync(_options.BucketName, key, cancellationToken).ConfigureAwait(false);
        }
        catch (AmazonS3Exception ex)
        {
            throw new FileStorageException($"Failed to delete '{key}' from S3", ex);
        }
    }

    public async ValueTask<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        StorageKey.Validate(key);
        try
        {
            await _client.GetObjectMetadataAsync(_options.BucketName, key, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async ValueTask<FileMetadata?> GetMetadataAsync(string key, CancellationToken cancellationToken = default)
    {
        StorageKey.Validate(key);
        try
        {
            var meta = await _client.GetObjectMetadataAsync(_options.BucketName, key, cancellationToken).ConfigureAwait(false);
            Dictionary<string, string>? userMeta = null;
            foreach (var k in meta.Metadata.Keys)
            {
                userMeta ??= new Dictionary<string, string>();
                userMeta[k] = meta.Metadata[k];
            }
            return new FileMetadata(
                Size: meta.ContentLength,
                ContentType: meta.Headers.ContentType ?? "application/octet-stream",
                LastModified: meta.LastModified ?? DateTimeOffset.UtcNow,
                UserMetadata: userMeta);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}
