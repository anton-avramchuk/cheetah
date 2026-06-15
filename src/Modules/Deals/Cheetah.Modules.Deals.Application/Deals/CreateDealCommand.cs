using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Core.Events;
using Cheetah.Modules.Deals.Domain.Abstractions;
using Cheetah.Modules.Deals.Domain.Entities;
using Cheetah.Modules.Deals.Shared;

namespace Cheetah.Modules.Deals.Application.Deals;

/// <summary>Создать сделку. Стартовая стадия — первая Open-стадия указанной воронки.</summary>
public sealed record CreateDealCommand(
    string Title,
    Guid PipelineId,
    decimal Amount,
    string Currency,
    Guid CustomerId,
    Guid OwnerId,
    Guid? ContactId,
    DateTimeOffset? ExpectedCloseDate) : ICommand<Guid>;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateDealCommand, Guid>))]
public sealed class CreateDealCommandHandler : ICommandHandler<CreateDealCommand, Guid>
{
    private readonly IRepository<Deal, Guid> _deals;
    private readonly IPipelineRepository _pipelines;
    private readonly IEventBus _eventBus;

    public CreateDealCommandHandler(
        IRepository<Deal, Guid> deals,
        IPipelineRepository pipelines,
        IEventBus eventBus)
    {
        _deals = deals;
        _pipelines = pipelines;
        _eventBus = eventBus;
    }

    public async ValueTask<Guid> HandleAsync(CreateDealCommand command, CancellationToken ct = default)
    {
        var pipeline = await _pipelines.GetWithStagesAsync(command.PipelineId, ct)
            ?? throw EntityNotFoundException.For<Pipeline>(command.PipelineId);

        var deal = Deal.Create(
            command.Title, pipeline, new Money(command.Amount, command.Currency),
            command.CustomerId, command.OwnerId, command.ContactId, command.ExpectedCloseDate);

        _deals.Add(deal);

        foreach (var e in deal.DomainEvents)
            await _eventBus.PublishAsync(e, ct);
        deal.ClearDomainEvents();

        await _deals.SaveChangesAsync(ct);
        return deal.Id;
    }
}
