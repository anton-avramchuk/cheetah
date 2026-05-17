# Cheetah.FileStorage.S3

S3-совместимое файловое хранилище для [Cheetah.FileStorage](../Cheetah.FileStorage/README.md). Работает с AWS S3, MinIO, Yandex Object Storage, Backblaze B2, Cloudflare R2 и любым другим S3-API.

## Состав

| Тип | Назначение |
|-----|------------|
| `S3FileStorage` | `IFileStorage` через AWSSDK.S3 |
| `S3PresignedUrlProvider` | `IFileStorageUrlProvider` — presigned URLs для прямой загрузки/скачивания клиентом |
| `S3FileStorageOptions` | `BucketName`, `Region`, `ServiceUrl`, `AccessKey`, `SecretKey`, `UsePathStyle` |
| `CrmFileStorageS3Module` | Регистрирует `IAmazonS3` и оба сервиса |

## Подключение

```csharp
[DependsOn(typeof(CrmFileStorageModule))]
[DependsOn(typeof(CrmFileStorageS3Module))]
public partial class MyAppModule : CrmModule { }
```

### AWS S3
```json
"FileStorage": {
  "S3": {
    "BucketName": "my-app-prod",
    "Region": "eu-central-1",
    "AccessKey": "AKIA...",
    "SecretKey": "..."
  }
}
```
Если `AccessKey`/`SecretKey` не указаны — используется стандартная AWS credentials chain (env-vars, `~/.aws/credentials`, IAM role).

### MinIO / dev
```json
"FileStorage": {
  "S3": {
    "BucketName": "dev-bucket",
    "ServiceUrl": "http://localhost:9000",
    "UsePathStyle": true,
    "AccessKey": "minioadmin",
    "SecretKey": "minioadmin"
  }
}
```

### Yandex Object Storage
```json
"FileStorage": {
  "S3": {
    "BucketName": "my-bucket",
    "ServiceUrl": "https://storage.yandexcloud.net",
    "AccessKey": "YCAJE...",
    "SecretKey": "..."
  }
}
```

## Presigned URLs

```csharp
public class DownloadController(IFileStorageUrlProvider urls)
{
    [HttpGet("/files/{key}/url")]
    public async Task<Uri> GetUrl(string key)
        => await urls.GetReadUrlAsync(key, TimeSpan.FromMinutes(15));
}
```

Так клиент скачивает файл напрямую из S3, минуя ваш бекенд — экономит трафик и CPU.
