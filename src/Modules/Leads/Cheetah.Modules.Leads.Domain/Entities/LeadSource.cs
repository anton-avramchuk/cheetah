using Cheetah.Core.Domain;

namespace Cheetah.Modules.Leads.Domain.Entities;

/// <summary>
/// Справочник источников лида (lookup-сущность, не абстрактная). Хранится в своей таблице с seed-данными
/// известных источников (<see cref="Cheetah.Modules.Leads.Shared.LeadSourceCodes"/>). Источники можно
/// добавлять как данные, без миграций кода.
/// </summary>
public sealed class LeadSource : Entity<Guid>
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public int Order { get; private set; }
    public bool IsActive { get; private set; }

    private LeadSource() { } // EF

    public static LeadSource Create(Guid id, string code, string name, int order, bool isActive = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new LeadSource
        {
            Id = id, Code = code.Trim(), Name = name.Trim(), Order = order, IsActive = isActive
        };
    }
}
