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

    private PermissionDefinition() { } // EF

    public static PermissionDefinition Create(string key, string? description, string? module)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Permission key cannot be empty", nameof(key));
        return new PermissionDefinition
        {
            Id = key,
            Description = description ?? "",
            Module = module ?? ""
        };
    }

    public void Update(string? description, string? module)
    {
        Description = description ?? "";
        Module = module ?? "";
    }
}
