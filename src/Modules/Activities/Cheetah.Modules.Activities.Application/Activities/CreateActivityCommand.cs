using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Modules.Activities.Application.Abstractions;
using Cheetah.Modules.Activities.Contracts;
using Cheetah.Modules.Activities.Domain.Entities;

namespace Cheetah.Modules.Activities.Application.Activities;

/// <summary>Создать активность из запроса наследника.</summary>
public sealed record CreateActivityCommand<TCreateRequest>(TCreateRequest Request) : ICommand<Guid>
    where TCreateRequest : CreateActivityRequestBase;

public class CreateActivityCommandHandler<TActivity, TCreateRequest>
    : ICommandHandler<CreateActivityCommand<TCreateRequest>, Guid>
    where TActivity : ActivityBase
    where TCreateRequest : CreateActivityRequestBase
{
    private readonly IActivityFactory<TActivity, TCreateRequest> _factory;
    private readonly IRepository<TActivity, Guid> _repository;
    private readonly IEventBus _eventBus;

    public CreateActivityCommandHandler(
        IActivityFactory<TActivity, TCreateRequest> factory,
        IRepository<TActivity, Guid> repository,
        IEventBus eventBus)
    {
        _factory = factory;
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask<Guid> HandleAsync(CreateActivityCommand<TCreateRequest> command, CancellationToken ct = default)
    {
        var activity = _factory.Create(command.Request);
        _repository.Add(activity);
        await _repository.SaveChangesAsync(ct);

        foreach (var e in activity.DomainEvents)
            await _eventBus.PublishAsync(e, ct);
        activity.ClearDomainEvents();

        return activity.Id;
    }
}
