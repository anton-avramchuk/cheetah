using Amazon.S3;
using Amazon.S3.Model;
using Cheetah.Core.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cheetah.FileStorage.S3;

/// <summary>
/// Реализация IFileStorageUrlProvider через S3 presigned URLs.
/// Позволяет клиентам качать/загружать файлы напрямую, минуя ваш бекенд.
/// </summary>
[Export(LifetimeType.Singleton, typeof(IFileStorageUrlProvider))]
public sealed class S3PresignedUrlProvider : IFileStorageUrlProvider
{
    private readonly IAmazonS3 _client;
    private readonly S3FileStorageOptions _options;

    public S3PresignedUrlProvider(IAmazonS3 client, IOptions<S3FileStorageOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    public async ValueTask<Uri> GetReadUrlAsync(string key, TimeSpan validFor, CancellationToken cancellationToken = default)
    {
        StorageKey.Validate(key);
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.Add(validFor)
        };
        var url = await _client.GetPreSignedURLAsync(request).ConfigureAwait(false);
        return new Uri(url);
    }

    public async ValueTask<Uri> GetUploadUrlAsync(string key, TimeSpan validFor, string contentType, CancellationToken cancellationToken = default)
    {
        StorageKey.Validate(key);
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            Verb = HttpVerb.PUT,
            ContentType = contentType,
            Expires = DateTime.UtcNow.Add(validFor)
        };
        var url = await _client.GetPreSignedURLAsync(request).ConfigureAwait(false);
        return new Uri(url);
    }
}
