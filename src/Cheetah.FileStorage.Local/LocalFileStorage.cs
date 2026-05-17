using System.Text.Json;
using Cheetah.Core.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.FileStorage.Local;

/// <summary>
/// Файловое хранилище поверх локальной FS. Ключ маппится на относительный путь от RootPath.
/// StorageKey.Validate защищает от path-traversal; дополнительно проверяется, что resolved path
/// остаётся внутри RootPath.
/// </summary>
[Export(LifetimeType.Singleton, typeof(IFileStorage))]
public sealed class LocalFileStorage : IFileStorage
{
    private readonly LocalFileStorageOptions _options;
    private readonly ILogger<LocalFileStorage> _logger;
    private readonly string _rootFullPath;

    public LocalFileStorage(IOptions<LocalFileStorageOptions> options, ILogger<LocalFileStorage> logger)
    {
        _options = options.Value;
        _logger = logger;
        _rootFullPath = Path.GetFullPath(_options.RootPath);
    }

    public async ValueTask SaveAsync(string key, Stream content, string contentType,
        IReadOnlyDictionary<string, string>? userMetadata = null,
        CancellationToken cancellationToken = default)
    {
        var path = ResolveSafe(key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None,
                         bufferSize: 81920, useAsync: true))
        {
            await content.CopyToAsync(fs, cancellationToken).ConfigureAwait(false);
        }

        if (_options.StoreContentTypeSidecar)
        {
            var sidecar = new SidecarMetadata(contentType, userMetadata);
            await File.WriteAllTextAsync(SidecarPath(path),
                JsonSerializer.Serialize(sidecar), cancellationToken).ConfigureAwait(false);
        }

        _logger.LogDebug("Saved file '{Key}' ({Size} bytes)", key, new FileInfo(path).Length);
    }

    public ValueTask<Stream> OpenReadAsync(string key, CancellationToken cancellationToken = default)
    {
        var path = ResolveSafe(key);
        if (!File.Exists(path))
            throw new FileStorageNotFoundException(key);

        Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 81920, useAsync: true);
        return ValueTask.FromResult(stream);
    }

    public ValueTask DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        var path = ResolveSafe(key);
        if (File.Exists(path))
        {
            File.Delete(path);
            var sidecar = SidecarPath(path);
            if (File.Exists(sidecar)) File.Delete(sidecar);
            _logger.LogDebug("Deleted file '{Key}'", key);
        }
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        => ValueTask.FromResult(File.Exists(ResolveSafe(key)));

    public async ValueTask<FileMetadata?> GetMetadataAsync(string key, CancellationToken cancellationToken = default)
    {
        var path = ResolveSafe(key);
        if (!File.Exists(path)) return null;

        var info = new FileInfo(path);
        var contentType = "application/octet-stream";
        IReadOnlyDictionary<string, string>? userMetadata = null;

        if (_options.StoreContentTypeSidecar)
        {
            var sidecarPath = SidecarPath(path);
            if (File.Exists(sidecarPath))
            {
                var json = await File.ReadAllTextAsync(sidecarPath, cancellationToken).ConfigureAwait(false);
                var sidecar = JsonSerializer.Deserialize<SidecarMetadata>(json);
                if (sidecar is not null)
                {
                    contentType = sidecar.ContentType;
                    userMetadata = sidecar.UserMetadata;
                }
            }
        }

        return new FileMetadata(info.Length, contentType, info.LastWriteTimeUtc, userMetadata);
    }

    private string ResolveSafe(string key)
    {
        StorageKey.Validate(key);
        var combined = Path.GetFullPath(Path.Combine(_rootFullPath, key.Replace('/', Path.DirectorySeparatorChar)));

        // Defence in depth: даже если StorageKey.Validate пропустил какой-то edge case,
        // проверяем что resolved path внутри RootPath.
        if (!combined.StartsWith(_rootFullPath + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            && combined != _rootFullPath)
        {
            throw new ArgumentException($"Key '{key}' resolves outside RootPath", nameof(key));
        }
        return combined;
    }

    private static string SidecarPath(string filePath) => filePath + ".meta.json";

    private sealed record SidecarMetadata(string ContentType, IReadOnlyDictionary<string, string>? UserMetadata);
}
