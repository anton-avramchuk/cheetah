using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Calendar.Contracts;
using Cheetah.Modules.Calendar.Domain.Entities;
using Cheetah.Modules.Calendar.Domain.Specifications;

namespace Cheetah.Modules.Calendar.Application.Registry;

/// <summary>
/// Идемпотентный upsert реестра привязываемых типов (вызывает Client потребителя при старте).
/// </summary>
public sealed record SyncCalendarRegistryCommand(CalendarRegistrySyncRequest Request) : ICommand;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<SyncCalendarRegistryCommand>))]
public sealed class SyncCalendarRegistryCommandHandler : ICommandHandler<SyncCalendarRegistryCommand>
{
    private readonly IRepository<CalendarableEntityType, Guid> _types;

    public SyncCalendarRegistryCommandHandler(IRepository<CalendarableEntityType, Guid> types) => _types = types;

    public async ValueTask HandleAsync(SyncCalendarRegistryCommand command, CancellationToken ct = default)
    {
        foreach (var item in command.Request.Items)
        {
            var owner = item.OwnerService ?? command.Request.OwnerService;
            var existing = await _types.GetBySpecAsync(new CalendarableTypeByKeySpecification(item.EntityType), ct);
            if (existing is null)
            {
                _types.Add(CalendarableEntityType.Create(
                    item.EntityType, item.DisplayName, item.DefaultColor, item.AllowMultiplePerEntity, owner));
            }
            else
            {
                existing.Update(item.DisplayName, item.DefaultColor, item.AllowMultiplePerEntity, owner);
            }
        }

        await _types.SaveChangesAsync(ct);
    }
}
