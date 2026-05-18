using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Permissions.Catalog.Domain;

namespace Cheetah.Permissions.Catalog.Application;

/// <summary>
/// Сохранить в каталог набор permissions для конкретного модуля. Идемпотентна:
/// существующие записи обновляются, новые создаются. Permissions того же модуля,
/// которых нет в Items — НЕ удаляются (модуль может пройти sync частично).
/// </summary>
public sealed record SyncRegistryCommand(string Module, IReadOnlyList<PermissionDefinitionDto> Items) : ICommand;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<SyncRegistryCommand>))]
public class SyncRegistryCommandHandler : ICommandHandler<SyncRegistryCommand>
{
    private readonly IRepository<PermissionDefinition, string> _repository;

    public SyncRegistryCommandHandler(IRepository<PermissionDefinition, string> repository)
        => _repository = repository;

    public async ValueTask HandleAsync(SyncRegistryCommand command, CancellationToken ct = default)
    {
        var keys = command.Items.Select(i => i.Key).ToArray();
        var existing = (await _repository.GetAllAsync(new PermissionsByKeysSpecification(keys), ct))
            .ToDictionary(p => p.Id);

        foreach (var item in command.Items)
        {
            if (existing.TryGetValue(item.Key, out var pd))
                pd.Update(item.Description, command.Module);
            else
                _repository.Add(PermissionDefinition.Create(item.Key, item.Description, command.Module));
        }

        await _repository.SaveChangesAsync(ct);
    }
}
