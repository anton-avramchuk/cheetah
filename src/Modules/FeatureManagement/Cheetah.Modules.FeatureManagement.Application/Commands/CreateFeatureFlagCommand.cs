using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Modules.FeatureManagement.Application.Abstractions;
using Cheetah.Modules.FeatureManagement.Contracts;
using Cheetah.Modules.FeatureManagement.Domain.Entities;

namespace Cheetah.Modules.FeatureManagement.Application.Commands;

/// <summary>Создать флаг из запроса админки наследника.</summary>
public sealed record CreateFeatureFlagCommand<TCreateRequest>(TCreateRequest Request) : ICommand<Guid>
    where TCreateRequest : CreateFeatureFlagRequestBase;

public class CreateFeatureFlagCommandHandler<TFlag, TCreateRequest>
    : ICommandHandler<CreateFeatureFlagCommand<TCreateRequest>, Guid>
    where TFlag : FeatureFlagBase
    where TCreateRequest : CreateFeatureFlagRequestBase
{
    private readonly IFeatureFlagFactory<TFlag, TCreateRequest> _factory;
    private readonly IRepository<TFlag, Guid> _repository;
    private readonly IEventBus _eventBus;

    public CreateFeatureFlagCommandHandler(
        IFeatureFlagFactory<TFlag, TCreateRequest> factory,
        IRepository<TFlag, Guid> repository,
        IEventBus eventBus)
    {
        _factory = factory;
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask<Guid> HandleAsync(CreateFeatureFlagCommand<TCreateRequest> command, CancellationToken ct = default)
    {
        var flag = _factory.Create(command.Request);
        _repository.Add(flag);

        // Публикация ДО SaveChanges: OutboxEventBus пишет в outbox того же DbContext —
        // флаг и события коммитятся атомарно.
        foreach (var e in flag.DomainEvents)
            await _eventBus.PublishAsync(e, ct);
        flag.ClearDomainEvents();

        await _repository.SaveChangesAsync(ct);
        return flag.Id;
    }
}
