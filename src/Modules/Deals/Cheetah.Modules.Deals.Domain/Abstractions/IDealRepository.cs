using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Specification;
using Cheetah.Modules.Deals.Domain.Entities;

namespace Cheetah.Modules.Deals.Domain.Abstractions;

/// <summary>
/// Репозиторий сделок с операциями, требующими серверной агрегации/пагинации
/// (горячие пути доски и списков — план §6.1, §9).
/// </summary>
public interface IDealRepository : IRepository<Deal, Guid>
{
    /// <summary>Агрегаты по открытым сделкам воронки, сгруппированные по стадии (для Kanban-доски).</summary>
    ValueTask<IReadOnlyList<StageAggregate>> GetOpenBoardAsync(Guid pipelineId, CancellationToken ct = default);

    /// <summary>Постраничный список сделок по спецификации (read-only, без трекинга).</summary>
    ValueTask<List<Deal>> ListAsync(ISpecification<Deal> spec, int page, int size, CancellationToken ct = default);
}

/// <summary>Агрегат стадии для Kanban-доски: количество и сумма открытых сделок.</summary>
public readonly record struct StageAggregate(Guid StageId, int Count, decimal Sum);
