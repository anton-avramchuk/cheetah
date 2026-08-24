using Cheetah.Core.Specification;
using Cheetah.Modules.Workflow.Domain.Entities;

namespace Cheetah.Modules.Workflow.Domain.Repositories;

/// <summary>
/// Дочерне-осведомлённый репозиторий правил: умеет грузить агрегат вместе с триггерами и действиями
/// (нужно для <c>Enable</c>/проекции/исполнения). Реализация — в Infrastructure (EF <c>Include</c>).
/// </summary>
public interface IAutomationRuleRepository<TRule> where TRule : AutomationRuleBase
{
    ValueTask<TRule?> GetByIdAsync(Guid id, bool includeChildren, CancellationToken ct = default);
    ValueTask<IReadOnlyList<TRule>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, bool includeChildren, CancellationToken ct = default);
    ValueTask<IReadOnlyList<TRule>> ListAsync(ISpecification<TRule>? spec, bool includeChildren, CancellationToken ct = default);

    /// <summary>
    /// Страница правил: отбор, сортировка и срез выполняются в БД. Порядок — новые сверху,
    /// завершается <c>Id</c>: без тай-брейкера страницы дублируют и теряют строки.
    /// </summary>
    ValueTask<IReadOnlyList<TRule>> ListPageAsync(
        ISpecification<TRule>? spec, bool includeChildren, int skip, int take, CancellationToken ct = default);
    void Add(TRule rule);
    void Delete(TRule rule);
    ValueTask<int> SaveChangesAsync(CancellationToken ct = default);
}
