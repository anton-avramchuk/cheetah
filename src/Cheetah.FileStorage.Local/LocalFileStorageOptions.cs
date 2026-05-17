namespace Cheetah.FileStorage.Local;

public class LocalFileStorageOptions
{
    /// <summary>
    /// Корневая папка для всех файлов. Должна существовать (или будет создана при первом SaveAsync).
    /// </summary>
    public string RootPath { get; set; } = "files";

    /// <summary>
    /// Сохранять ContentType в sidecar-файл &lt;name&gt;.meta.json рядом.
    /// Если false — GetMetadataAsync вернёт ContentType = "application/octet-stream".
    /// </summary>
    public bool StoreContentTypeSidecar { get; set; } = true;
}
