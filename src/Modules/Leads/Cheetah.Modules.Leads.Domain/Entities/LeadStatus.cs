using Cheetah.Core.Domain;

namespace Cheetah.Modules.Leads.Domain.Entities;

/// <summary>
/// Справочник статусов лида (lookup-сущность, не абстрактная). Хранится в своей таблице с seed-данными
/// известных статусов (<see cref="Cheetah.Modules.Leads.Shared.LeadStatusCodes"/>). Статус лида —
/// данные, а не enum; доменные переходы опираются на стабильные well-known идентификаторы.
/// </summary>
public sealed class LeadStatus : Entity<Guid>
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public int Order { get; private set; }
    public bool IsActive { get; private set; }

    /// <summary>Терминальный статус (Converted/Disqualified) — лид завершил воронку.</summary>
    public bool IsTerminal { get; private set; }

    private LeadStatus() { } // EF

    public static LeadStatus Create(Guid id, string code, string name, int order, bool isTerminal, bool isActive = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new LeadStatus
        {
            Id = id, Code = code.Trim(), Name = name.Trim(),
            Order = order, IsTerminal = isTerminal, IsActive = isActive
        };
    }
}
