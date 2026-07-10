using Cheetah.Core.Domain;

namespace Cheetah.Permissions.Catalog.Domain;

/// <summary>
/// Запись каталога: объявленный в системе permission. Используется UI-админом для выбора,
/// какой permission назначить какой роли. Источник истины — код модулей (атрибут [Permission]).
/// Каталог — это денормализованная проекция этого набора для админ-операций.
/// </summary>
public class PermissionDefinition : Entity<string>
{
    public string Description { get; private set; } = "";
    public string Module { get; private set; } = "";

    /// <summary>
    /// Ключ фич-флага, за которым спрятан permission. Пока фича выключена (или её нет в каталоге фич),
    /// permission не отдаётся в списке. Пусто/null — permission виден всегда.
    /// </summary>
    public string? Feature { get; private set; }

    private PermissionDefinition() { } // EF

    public static PermissionDefinition Create(string key, string? description, string? module, string? feature = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Permission key cannot be empty", nameof(key));
        return new PermissionDefinition
        {
            Id = key,
            Description = description ?? "",
            Module = module ?? "",
            Feature = Normalize(feature)
        };
    }

    public void Update(string? description, string? module, string? feature = null)
    {
        Description = description ?? "";
        Module = module ?? "";
        Feature = Normalize(feature);
    }

    // Пустая строка из БД/DTO — это «фичи нет», а не фича с пустым ключом.
    private static string? Normalize(string? feature)
        => string.IsNullOrWhiteSpace(feature) ? null : feature;
}
