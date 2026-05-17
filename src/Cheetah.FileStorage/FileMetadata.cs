namespace Cheetah.FileStorage;

/// <summary>
/// Метаданные файла в хранилище.
/// </summary>
public sealed record FileMetadata(
    long Size,
    string ContentType,
    DateTimeOffset LastModified,
    IReadOnlyDictionary<string, string>? UserMetadata = null);
