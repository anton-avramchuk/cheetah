namespace Cheetah.FileStorage;

/// <summary>
/// Абстракция файлового хранилища. Ключ (<paramref name="key"/>) — opaque-строка
/// с иерархическими сегментами через "/" (например "attachments/2026/05/uuid-name.pdf").
/// Конкретные провайдеры (Local, S3, Azure Blob) реализуют эту абстракцию по-своему.
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// Сохранить содержимое <paramref name="content"/> под ключом <paramref name="key"/>.
    /// Если файл с таким ключом уже существует — перезаписывается.
    /// </summary>
    ValueTask SaveAsync(string key, Stream content, string contentType,
        IReadOnlyDictionary<string, string>? userMetadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Открыть поток на чтение. Поток должен быть закрыт вызывающим.
    /// Бросает <see cref="FileStorageNotFoundException"/> если файла нет.
    /// </summary>
    ValueTask<Stream> OpenReadAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить файл. Идемпотентно: если файла нет — не бросает.
    /// </summary>
    ValueTask DeleteAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Существует ли файл под этим ключом.
    /// </summary>
    ValueTask<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить метаданные. Возвращает null если файла нет.
    /// </summary>
    ValueTask<FileMetadata?> GetMetadataAsync(string key, CancellationToken cancellationToken = default);
}

/// <summary>
/// Опциональная абстракция для провайдеров, поддерживающих presigned URLs
/// (S3, Azure Blob). Для локального FS не реализуется.
/// </summary>
public interface IFileStorageUrlProvider
{
    /// <summary>
    /// Сгенерировать временный URL на чтение файла.
    /// </summary>
    ValueTask<Uri> GetReadUrlAsync(string key, TimeSpan validFor, CancellationToken cancellationToken = default);

    /// <summary>
    /// Сгенерировать временный URL для загрузки файла напрямую с клиента.
    /// </summary>
    ValueTask<Uri> GetUploadUrlAsync(string key, TimeSpan validFor, string contentType, CancellationToken cancellationToken = default);
}
