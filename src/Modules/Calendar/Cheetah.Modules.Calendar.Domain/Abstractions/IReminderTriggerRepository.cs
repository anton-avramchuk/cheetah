using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Modules.Calendar.Domain.Entities;

namespace Cheetah.Modules.Calendar.Domain.Abstractions;

/// <summary>Репозиторий материализованных срабатываний напоминаний с батч-выборкой «пора слать».</summary>
public interface IReminderTriggerRepository : IRepository<ReminderTrigger, Guid>
{
    /// <summary>
    /// Срабатывания, чей момент уже наступил, ограниченным батчем и в порядке времени —
    /// горячий путь скана планировщика.
    /// </summary>
    ValueTask<List<ReminderTrigger>> GetDueAsync(DateTime nowUtc, int batchSize, CancellationToken cancellationToken = default);
}
