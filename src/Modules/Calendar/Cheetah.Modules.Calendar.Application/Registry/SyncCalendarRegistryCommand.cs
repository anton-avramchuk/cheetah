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
public sealed record SyncCalendarRegistryCommand(
    string OwnerService, IReadOnlyList<CalendarableEntityTypeRegistration> Items) : ICommand;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<SyncCalendarRegistryCommand>))]
public sealed class SyncCalendarRegistryCommandHandler : ICommandHandler<SyncCalendarRegistryCommand>
{
    private readonly IRepository<CalendarableEntityType, Guid> _types;

    public SyncCalendarRegistryCommandHandler(IRepository<CalendarableEntityType, Guid> types) => _types = types;

    public async ValueTask HandleAsync(SyncCalendarRegistryCommand command, CancellationToken ct = default)
    {
        var keys = command.Items.Select(i => i.EntityType).Distinct().ToArray();

        // Один запрос на весь батч вместо N точечных lookup-ов.
        var existing = await _types.GetAllAsync(new CalendarableTypesByKeysSpecification(keys), ct);
        var existingByKey = existing.ToDictionary(t => t.EntityType);

        foreach (var item in command.Items)
        {
            var owner = item.OwnerService ?? command.OwnerService;
            if (existingByKey.TryGetValue(item.EntityType, out var type))
            {
                type.Update(item.DisplayName, item.DefaultColor, item.AllowMultiplePerEntity, owner);
            }
            else
            {
                _types.Add(CalendarableEntityType.Create(
                    item.EntityType, item.DisplayName, item.DefaultColor, item.AllowMultiplePerEntity, owner));
            }
        }

        await _types.SaveChangesAsync(ct);
    }
}
