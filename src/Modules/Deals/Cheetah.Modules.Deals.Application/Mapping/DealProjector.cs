using Cheetah.Modules.Deals.Contracts;
using Cheetah.Modules.Deals.Domain.Entities;

namespace Cheetah.Modules.Deals.Application.Mapping;

/// <summary>
/// Проекция доменных сущностей в DTO. Без Mapster — owned-VO <c>Money</c> и коллекции детей
/// раскрываются явно.
/// </summary>
internal static class DealProjector
{
    public static DealDto ToDto(Deal d)
        => new(
            d.Id, d.Title, d.PipelineId, d.StageId,
            d.Value.Amount, d.Value.Currency,
            d.CustomerId, d.ContactId, d.OwnerId,
            d.ExpectedCloseDate, d.Status, d.LostReason, d.ClosedAt,
            d.CreatedAt, d.UpdatedAt);

    public static DealListItemDto ToListItem(Deal d)
        => new(
            d.Id, d.Title, d.PipelineId, d.StageId,
            d.Value.Amount, d.Value.Currency,
            d.CustomerId, d.OwnerId, d.Status, d.ExpectedCloseDate);

    public static DealHistoryDto ToDto(DealStageHistory h)
        => new(h.Id, h.FromStageId, h.ToStageId, h.ChangedBy, h.CreatedAt);

    public static PipelineDto ToDto(Pipeline p)
        => new(
            p.Id, p.Name, p.IsDefault, p.IsActive,
            p.Stages.OrderBy(s => s.Order).Select(ToDto).ToArray());

    public static PipelineStageDto ToDto(PipelineStage s)
        => new(s.Id, s.Name, s.Order, s.Probability, s.Type);
}
