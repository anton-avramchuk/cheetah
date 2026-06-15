using Cheetah.BackgroundTasks;
using Cheetah.Core.DependencyInjection;
using Cheetah.DistributedLock;
using Cheetah.Modules.Calendar.Application.Abstractions;
using Cheetah.Modules.Calendar.Application.Options;
using Cheetah.Modules.Calendar.Domain.Abstractions;
using Cheetah.Modules.Calendar.Domain.Specifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Modules.Calendar.Application.BackgroundTasks;

/// <summary>
/// Подметание горизонта: периодически докатывает материализованные срабатывания напоминаний для
/// повторяющихся событий на <c>HorizonDays</c> вперёд. Разовые события материализуются в момент
/// команды; бесконечные серии нуждаются в этом фоновом досеве. Под distributed-lock.
/// </summary>
[Export(LifetimeType.Singleton, typeof(IBackgroundTask))]
public sealed class MaterializeRemindersTask : PeriodicBackgroundTask
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly CalendarReminderOptions _options;
    private readonly ILogger<MaterializeRemindersTask> _logger;

    public MaterializeRemindersTask(
        IServiceScopeFactory scopeFactory,
        IOptions<CalendarReminderOptions> options,
        ILogger<MaterializeRemindersTask> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    public override TimeSpan Period => _options.MaterializePeriod;

    public override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var sp = scope.ServiceProvider;

        var lockProvider = sp.GetService<IDistributedLockProvider>();
        IDistributedLock? heldLock = null;
        if (lockProvider is not null)
        {
            heldLock = await lockProvider.TryAcquireAsync(_options.MaterializeLockKey, cancellationToken);
            if (heldLock is null)
                return;
        }

        try
        {
            var events = sp.GetRequiredService<ICalendarEventRepository>();
            var triggers = sp.GetRequiredService<IReminderTriggerRepository>();
            var scheduler = sp.GetRequiredService<IReminderScheduler>();

            var recurring = await events.ListWithDetailsAsync(new RecurringEventsSpecification(), cancellationToken);
            if (recurring.Count == 0)
                return;

            var horizonEnd = DateTime.UtcNow.AddDays(_options.HorizonDays);
            foreach (var @event in recurring.Where(e => e.Reminders.Count > 0))
                await scheduler.RebuildAsync(@event, horizonEnd, cancellationToken: cancellationToken);

            await triggers.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Calendar reminder horizon materialized for {Count} recurring events", recurring.Count);
        }
        finally
        {
            if (heldLock is not null)
                await heldLock.DisposeAsync();
        }
    }
}
