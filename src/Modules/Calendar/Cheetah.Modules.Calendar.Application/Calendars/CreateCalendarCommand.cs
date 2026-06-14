using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Calendar.Contracts;
using Cheetah.Modules.Calendar.Shared;

namespace Cheetah.Modules.Calendar.Application.Calendars;

/// <summary>Создать календарь.</summary>
public sealed record CreateCalendarCommand(
    string Name,
    CalendarType Type,
    Guid OwnerUserId,
    string DefaultTimeZoneId,
    string? Color) : ICommand<Guid>;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateCalendarCommand, Guid>))]
public sealed class CreateCalendarCommandHandler : ICommandHandler<CreateCalendarCommand, Guid>
{
    private readonly IRepository<Domain.Entities.Calendar, Guid> _calendars;

    public CreateCalendarCommandHandler(IRepository<Domain.Entities.Calendar, Guid> calendars)
        => _calendars = calendars;

    public async ValueTask<Guid> HandleAsync(CreateCalendarCommand command, CancellationToken ct = default)
    {
        var calendar = Domain.Entities.Calendar.Create(
            command.Name, command.Type, command.OwnerUserId, command.DefaultTimeZoneId, command.Color);
        _calendars.Add(calendar);
        await _calendars.SaveChangesAsync(ct);
        return calendar.Id;
    }
}
