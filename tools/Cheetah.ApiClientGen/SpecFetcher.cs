using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace Cheetah.ApiClientGen;

/// <summary>
/// Fetches an OpenAPI document over HTTP with an ETag-based conditional GET, caches it on disk, and
/// reports whether the content changed. On a network failure it falls back to the cached copy so a
/// flaky/external service does not break the build.
/// </summary>
public sealed class SpecFetcher
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(15) };

    public sealed record FetchResult(string SpecPath, bool Changed, string Note);

    /// <summary>
    /// Ensures the latest spec for <paramref name="name"/> is on disk under <paramref name="cacheDir"/>.
    /// </summary>
    /// <param name="name">Logical client name; used for the cached file stem.</param>
    /// <param name="url">Source URL of the spec.</param>
    /// <param name="cacheDir">Directory holding the cached spec, its ETag and content hash.</param>
    /// <param name="fileExtension">Cached file extension, e.g. <c>.openapi.json</c> or <c>.proto</c>.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<FetchResult> FetchAsync(string name, string url, string cacheDir, string fileExtension, CancellationToken ct)
    {
        Directory.CreateDirectory(cacheDir);
        var specPath = Path.Combine(cacheDir, $"{name}{fileExtension}");
        var etagPath = Path.Combine(cacheDir, $"{name}.etag");
        var hashPath = Path.Combine(cacheDir, $"{name}.hash");

        var previousHash = File.Exists(hashPath) ? await File.ReadAllTextAsync(hashPath, ct) : null;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (File.Exists(etagPath) && File.Exists(specPath))
                request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(await File.ReadAllTextAsync(etagPath, ct)));

            using var response = await Http.SendAsync(request, ct);

            if (response.StatusCode == HttpStatusCode.NotModified && File.Exists(specPath))
                return new FetchResult(specPath, Changed: false, "304 Not Modified — using cached spec");

            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsByteArrayAsync(ct);
            await File.WriteAllBytesAsync(specPath, body, ct);

            if (response.Headers.ETag is { } etag)
                await File.WriteAllTextAsync(etagPath, etag.ToString(), ct);

            var hash = Sha256(body);
            await File.WriteAllTextAsync(hashPath, hash, ct);
            var changed = !string.Equals(hash, previousHash, StringComparison.Ordinal);
            return new FetchResult(specPath, changed, changed ? "downloaded (changed)" : "downloaded (unchanged)");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            if (File.Exists(specPath))
                return new FetchResult(specPath, Changed: false, $"fetch failed ({ex.GetType().Name}) — using cached spec");

            throw new InvalidOperationException(
                $"Failed to fetch spec for '{name}' from {url} and no cached copy exists.", ex);
        }
    }

    private static string Sha256(byte[] data)
    {
        var hash = SHA256.HashData(data);
        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}
