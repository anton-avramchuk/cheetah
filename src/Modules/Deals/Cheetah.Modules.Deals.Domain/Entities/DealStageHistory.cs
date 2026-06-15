using Cheetah.Core.Domain;

namespace Cheetah.Modules.Deals.Domain.Entities;

/// <summary>
/// Запись истории перехода сделки между стадиями — child-entity внутри <see cref="Deal"/>.
/// Время перехода берётся из <see cref="ICreateAtEntity.CreatedAt"/> (проставляется инфраструктурой).
/// </summary>
public sealed class DealStageHistory : Entity<Guid>, ICreateAtEntity
{
    public Guid DealId { get; private set; }
    public Guid FromStageId { get; private set; }
    public Guid ToStageId { get; private set; }
    public Guid ChangedBy { get; private set; }

    public DateTimeOffset? CreatedAt { get; set; }

    private DealStageHistory() { } // EF

    internal static DealStageHistory Create(Guid dealId, Guid fromStageId, Guid toStageId, Guid changedBy)
        => new()
        {
            Id = Guid.NewGuid(),
            DealId = dealId,
            FromStageId = fromStageId,
            ToStageId = toStageId,
            ChangedBy = changedBy
        };
}
