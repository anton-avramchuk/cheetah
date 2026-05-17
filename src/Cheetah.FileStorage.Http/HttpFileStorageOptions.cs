namespace Cheetah.FileStorage.Http;

public class HttpFileStorageOptions
{
    /// <summary>
    /// Базовый URL удалённого storage-сервиса. Ожидаемые endpoint'ы:
    /// PUT  {BaseUrl}/files/{key}              (Body: content, Content-Type: ...)
    /// GET  {BaseUrl}/files/{key}              -> 200 + content / 404
    /// DELETE {BaseUrl}/files/{key}            -> 204 (идемпотентно)
    /// HEAD   {BaseUrl}/files/{key}            -> 200 + Content-Length/Content-Type / 404
    /// GET  {BaseUrl}/files/{key}/metadata     -> JSON FileMetadata / 404
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>Bearer-токен для авторизации запросов. Опционально.</summary>
    public string? ApiKey { get; set; }

    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(2);
}
