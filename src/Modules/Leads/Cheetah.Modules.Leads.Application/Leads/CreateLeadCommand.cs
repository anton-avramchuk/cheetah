using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Modules.Leads.Application.Abstractions;
using Cheetah.Modules.Leads.Application.Exceptions;
using Cheetah.Modules.Leads.Contracts;
using Cheetah.Modules.Leads.Domain.Entities;
using Cheetah.Modules.Leads.Domain.Specifications;

namespace Cheetah.Modules.Leads.Application.Leads;

/// <summary>Создать лид из запроса наследника. Антидубль по Email/Phone — мягкое предупреждение.</summary>
public sealed record CreateLeadCommand<TCreateRequest>(TCreateRequest Request) : ICommand<Guid>
    where TCreateRequest : CreateLeadRequestBase;

public class CreateLeadCommandHandler<TLead, TCreateRequest>
    : ICommandHandler<CreateLeadCommand<TCreateRequest>, Guid>
    where TLead : LeadBase
    where TCreateRequest : CreateLeadRequestBase
{
    private readonly ILeadFactory<TLead, TCreateRequest> _factory;
    private readonly IRepository<TLead, Guid> _repository;
    private readonly IEventBus _eventBus;

    public CreateLeadCommandHandler(
        ILeadFactory<TLead, TCreateRequest> factory,
        IRepository<TLead, Guid> repository,
        IEventBus eventBus)
    {
        _factory = factory;
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask<Guid> HandleAsync(CreateLeadCommand<TCreateRequest> command, CancellationToken ct = default)
    {
        // Антидубль: при совпадении email — отклоняем (политику слияния можно вынести в наследника).
        if (!string.IsNullOrWhiteSpace(command.Request.Email)
            && await _repository.ExistsAsync(new LeadByEmailSpecification<TLead>(command.Request.Email), ct))
        {
            throw new LeadValidationException($"A lead with email '{command.Request.Email}' already exists.");
        }

        var lead = _factory.Create(command.Request);
        _repository.Add(lead);
        await _repository.SaveChangesAsync(ct);

        foreach (var e in lead.DomainEvents)
            await _eventBus.PublishAsync(e, ct);
        lead.ClearDomainEvents();

        return lead.Id;
    }
}
