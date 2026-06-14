using Cheetah.Modules.Tags.Contracts.Assignments;
using Cheetah.Modules.Tags.Contracts.Registry;
using Cheetah.Modules.Tags.Contracts.Tags;

namespace Cheetah.Modules.Tags.Client;

/// <summary>
/// HTTP-клиент к Tags.Api. Используется сервисами для регистрации своих применимых
/// типов сущностей при старте и для операций тэгирования server-to-server.
/// </summary>
public interface ITagsClient
{
    /// <summary>Зарегистрировать (upsert) применимые типы сущностей сервиса. Идемпотентно.</summary>
    ValueTask SyncRegistryAsync(RegistrySyncRequest request, CancellationToken ct = default);

    /// <summary>Назначить тэги сущности.</summary>
    ValueTask AssignAsync(AssignTagsRequest request, CancellationToken ct = default);

    /// <summary>Снять тэги с сущности.</summary>
    ValueTask UnassignAsync(UnassignTagsRequest request, CancellationToken ct = default);

    /// <summary>Получить тэги, назначенные сущности.</summary>
    ValueTask<IReadOnlyList<TagDto>> GetEntityTagsAsync(string entityType, Guid entityId, CancellationToken ct = default);
}
