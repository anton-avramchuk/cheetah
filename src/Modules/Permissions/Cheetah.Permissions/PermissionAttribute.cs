namespace Cheetah.Permissions;

/// <summary>
/// Декларативное объявление permission. Каждый модуль / микросервис помечает свои
/// permissions этим атрибутом — обычно на статических классах с константами:
///
/// <code>
/// public static class ContractPermissions
/// {
///     [Permission("Documents.Contract.Sign", "Подписать договор")]
///     public const string Sign = "Documents.Contract.Sign";
/// }
/// </code>
///
/// <see cref="PermissionRegistry"/> сканирует загруженные сборки и собирает их.
/// В монолите — синхронизирует с БД при старте. В микросервисах — отправляет
/// в Permissions.Catalog.Api через client.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property,
    AllowMultiple = false, Inherited = false)]
public sealed class PermissionAttribute : Attribute
{
    public string Key { get; }
    public string Description { get; }

    /// <summary>
    /// Опциональный логический модуль, к которому относится permission (для группировки в UI).
    /// Если не задан — берётся имя сборки.
    /// </summary>
    public string? Module { get; init; }

    /// <summary>
    /// Опциональный ключ фич-флага, к которому привязан permission. Пока фича выключена — или если
    /// такого флага вообще нет в каталоге — permission не показывается в каталоге
    /// (админу нечего назначать: функциональности не существует). Permission без фичи виден всегда.
    /// </summary>
    public string? Feature { get; init; }

    public PermissionAttribute(string key, string description = "")
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Permission key cannot be empty", nameof(key));
        Key = key;
        Description = description;
    }
}
