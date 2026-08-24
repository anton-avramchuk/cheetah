using Cheetah.Core.Specification;
using Cheetah.Modules.Workflow.Domain.Entities;

namespace Cheetah.Modules.Workflow.Domain.Repositories;

/// <summary>
/// Постраничное чтение журнала срабатываний. Журнал растёт неограниченно, поэтому отбор,
/// сортировка и срез обязаны выполняться в БД — материализовать его целиком нельзя.
/// </summary>
public interface IAutomationRunReader
{
    /// <summary>Страница прогонов: новые сверху, порядок завершается <c>Id</c>.</summary>
    ValueTask<IReadOnlyList<AutomationRun>> ListPageAsync(
        ISpecification<AutomationRun>? spec, int skip, int take, CancellationToken ct = default);
}
