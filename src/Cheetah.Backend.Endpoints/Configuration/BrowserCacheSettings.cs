namespace Cheetah.Backend.Endpoints.Configuration;

/// <summary>
/// Browser cache settings for Cache-Control response header
/// </summary>
/// <param name="MaxAgeSeconds">
/// Max age in seconds. Ignored when <see cref="NoStore"/> is true.
/// </param>
/// <param name="IsPublic">
/// true → Cache-Control: public (shared caches, e.g. CDN).
/// false → Cache-Control: private (browser only).
/// </param>
/// <param name="NoStore">
/// Emits Cache-Control: no-store, no-cache. Overrides MaxAgeSeconds and IsPublic.
/// </param>
public sealed record BrowserCacheSettings(
    int MaxAgeSeconds,
    bool IsPublic = false,
    bool NoStore = false);
