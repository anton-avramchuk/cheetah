using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Specification;
using Cheetah.Modules.Calendar.Domain.Entities;

namespace Cheetah.Modules.Calendar.Domain.Abstractions;

/// <summary>
/// Репозиторий событий с загрузкой полного агрегата (участники, напоминания, override-ы),
/// которую обычный <see cref="IRepository{TEntity,TKey}"/> не выполняет (FindAsync без Include).
/// </summary>
public interface ICalendarEventRepository : IRepository<CalendarEvent, Guid>
{
    /// <summary>Загрузить событие со всеми детьми агрегата.</summary>
    ValueTask<CalendarEvent?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Загрузить события по спецификации со всеми детьми агрегата.</summary>
    ValueTask<List<CalendarEvent>> ListWithDetailsAsync(ISpecification<CalendarEvent> spec, CancellationToken cancellationToken = default);
}
