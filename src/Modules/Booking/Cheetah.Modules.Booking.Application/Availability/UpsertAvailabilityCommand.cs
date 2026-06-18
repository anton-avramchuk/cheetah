using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Modules.Booking.Application.Abstractions;
using Cheetah.Modules.Booking.Contracts;
using Cheetah.Modules.Booking.Domain.Entities;
using Cheetah.Modules.Booking.Domain.Specifications;

namespace Cheetah.Modules.Booking.Application.Availability;

/// <summary>Создать/обновить недельную доступность host'а с исключениями (upsert по host'у).</summary>
public sealed record UpsertAvailabilityCommand(UpsertAvailabilityRequest Request) : ICommand<Guid>;

public class UpsertAvailabilityCommandHandler<TSchedule> : ICommandHandler<UpsertAvailabilityCommand, Guid>
    where TSchedule : AvailabilityScheduleBase
{
    private readonly IRepository<TSchedule, Guid> _repository;
    private readonly IScheduleFactory<TSchedule> _factory;

    public UpsertAvailabilityCommandHandler(IRepository<TSchedule, Guid> repository, IScheduleFactory<TSchedule> factory)
    {
        _repository = repository;
        _factory = factory;
    }

    public async ValueTask<Guid> HandleAsync(UpsertAvailabilityCommand command, CancellationToken ct = default)
    {
        var r = command.Request;
        var schedule = await _repository.GetBySpecAsync(new ScheduleByHostSpecification<TSchedule>(r.HostUserId), ct);

        var isNew = schedule is null;
        schedule ??= _factory.Create(r.HostUserId, r.TimeZoneId);
        schedule.SetTimeZone(r.TimeZoneId);

        schedule.ReplaceRules(
            r.WeeklyRules.Select(w => (w.DayOfWeek, w.StartTime, w.EndTime)),
            r.DateOverrides.Select(o => (o.Date, o.IsUnavailable, o.StartTime, o.EndTime)));

        if (isNew)
            _repository.Add(schedule);
        await _repository.SaveChangesAsync(ct);
        return schedule.Id;
    }
}
