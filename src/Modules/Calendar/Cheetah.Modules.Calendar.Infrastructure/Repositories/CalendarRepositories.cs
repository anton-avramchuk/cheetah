using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.EntityFramework.Repositories;
using Cheetah.Core.Specification;
using Cheetah.Modules.Calendar.Domain.Abstractions;
using Cheetah.Modules.Calendar.Domain.Entities;
using Cheetah.Modules.Calendar.Domain.Specifications;
using Cheetah.Modules.Calendar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.Calendar.Infrastructure.Repositories;

[Export(LifetimeType.Scoped, typeof(IRepository<Domain.Entities.Calendar, Guid>))]
public class CalendarRepository : EfRepository<CalendarDbContext, Domain.Entities.Calendar, Guid>
{
    public CalendarRepository(CalendarDbContext context) : base(context) { }
}

[Export(LifetimeType.Scoped, typeof(IRepository<CalendarEvent, Guid>), typeof(ICalendarEventRepository))]
public class CalendarEventRepository : EfRepository<CalendarDbContext, CalendarEvent, Guid>, ICalendarEventRepository
{
    public CalendarEventRepository(CalendarDbContext context) : base(context) { }

    public async ValueTask<CalendarEvent?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbSet
            .Include(e => e.Attendees)
            .Include(e => e.Reminders)
            .Include(e => e.Overrides)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async ValueTask<List<CalendarEvent>> ListWithDetailsAsync(
        ISpecification<CalendarEvent> spec, CancellationToken cancellationToken = default)
        => await DbSet
            .Include(e => e.Attendees)
            .Include(e => e.Reminders)
            .Include(e => e.Overrides)
            .Where(spec.ToExpression())
            .ToListAsync(cancellationToken);
}

[Export(LifetimeType.Scoped, typeof(IRepository<ReminderTrigger, Guid>), typeof(IReminderTriggerRepository))]
public class ReminderTriggerRepository : EfRepository<CalendarDbContext, ReminderTrigger, Guid>, IReminderTriggerRepository
{
    public ReminderTriggerRepository(CalendarDbContext context) : base(context) { }

    public async ValueTask<List<ReminderTrigger>> GetDueAsync(
        DateTime nowUtc, int batchSize, CancellationToken cancellationToken = default)
        => await DbSet
            .Where(new DueReminderTriggersSpecification(nowUtc).ToExpression())
            .OrderBy(t => t.FireAtUtc)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
}

[Export(LifetimeType.Scoped, typeof(IRepository<CalendarableEntityType, Guid>))]
public class CalendarableEntityTypeRepository : EfRepository<CalendarDbContext, CalendarableEntityType, Guid>
{
    public CalendarableEntityTypeRepository(CalendarDbContext context) : base(context) { }
}
