namespace Cheetah.FileStorage.S3;

public class S3FileStorageOptions
{
    public string BucketName { get; set; } = string.Empty;

    /// <summary>
    /// Регион AWS (например "eu-central-1"). Игнорируется если ServiceUrl задан.
    /// </summary>
    public string Region { get; set; } = "us-east-1";

    /// <summary>
    /// URL S3-compatible endpoint'а (MinIO, Yandex Object Storage, Backblaze B2 и т.п.).
    /// Если null — используется AWS S3 в указанном Region.
    /// Примеры: "https://storage.yandexcloud.net", "http://localhost:9000".
    /// </summary>
    public string? ServiceUrl { get; set; }

    public string? AccessKey { get; set; }
    public string? SecretKey { get; set; }

    /// <summary>
    /// Path-style addressing (bucket в URL, не в hostname). Нужно для MinIO и обычно для
    /// dev-сред. Для production AWS оставьте false (virtual-hosted style).
    /// </summary>
    public bool UsePathStyle { get; set; } = false;
}
