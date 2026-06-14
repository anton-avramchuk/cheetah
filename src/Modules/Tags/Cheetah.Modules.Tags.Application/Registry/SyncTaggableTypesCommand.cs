using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Tags.Contracts.Registry;
using Cheetah.Modules.Tags.Domain.Entities;
using Cheetah.Modules.Tags.Domain.Specifications;

namespace Cheetah.Modules.Tags.Application.Registry;

/// <summary>
/// Зарегистрировать (upsert) применимые к тэгам типы сущностей одного сервиса.
/// Идемпотентна: существующие типы обновляются, новые создаются. Типы того же сервиса,
/// которых нет в Items, НЕ удаляются (sync может быть частичным).
/// </summary>
public sealed record SyncTaggableTypesCommand(string OwnerService, IReadOnlyList<TaggableEntityTypeDto> Items) : ICommand;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<SyncTaggableTypesCommand>))]
public class SyncTaggableTypesCommandHandler : ICommandHandler<SyncTaggableTypesCommand>
{
    private readonly IRepository<TaggableEntityType, string> _repository;

    public SyncTaggableTypesCommandHandler(IRepository<TaggableEntityType, string> repository)
        => _repository = repository;

    public async ValueTask HandleAsync(SyncTaggableTypesCommand command, CancellationToken ct = default)
    {
        var keys = command.Items.Select(i => i.Key).ToArray();
        var existing = (await _repository.GetAllAsync(new TaggableTypesByKeysSpecification(keys), ct))
            .ToDictionary(t => t.Id);

        foreach (var item in command.Items)
        {
            if (existing.TryGetValue(item.Key, out var type))
            {
                type.Update(item.DisplayName, command.OwnerService,
                    item.MaxTagsPerEntity, item.AllowAdHocTags, item.AllowedGroups);
                _repository.Update(type);
            }
            else
            {
                _repository.Add(TaggableEntityType.Create(item.Key, item.DisplayName, command.OwnerService,
                    item.MaxTagsPerEntity, item.AllowAdHocTags, item.AllowedGroups));
            }
        }

        await _repository.SaveChangesAsync(ct);
    }
}
