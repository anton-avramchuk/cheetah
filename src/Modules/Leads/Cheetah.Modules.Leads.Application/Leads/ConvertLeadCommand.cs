using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Modules.Leads.Application.Exceptions;
using Cheetah.Modules.Leads.Contracts;
using Cheetah.Modules.Leads.Domain.Abstractions;
using Cheetah.Modules.Leads.Domain.Entities;
using Cheetah.Modules.Leads.Shared;

namespace Cheetah.Modules.Leads.Application.Leads;

/// <summary>
/// Сконвертировать лид в клиента (+ опц. сделку). Фактическое создание Customer/Deal делегируется
/// порту <see cref="ILeadConversionOrchestrator"/> (реализацию подключает наследник).
/// </summary>
public sealed record ConvertLeadCommand<TConvertRequest>(Guid Id, TConvertRequest Request)
    : ICommand<ConvertLeadResult>
    where TConvertRequest : ConvertLeadRequestBase;

public class ConvertLeadCommandHandler<TLead, TConvertRequest>
    : ICommandHandler<ConvertLeadCommand<TConvertRequest>, ConvertLeadResult>
    where TLead : LeadBase
    where TConvertRequest : ConvertLeadRequestBase
{
    private readonly IRepository<TLead, Guid> _repository;
    private readonly ILeadConversionOrchestrator _orchestrator;
    private readonly IEventBus _eventBus;

    public ConvertLeadCommandHandler(
        IRepository<TLead, Guid> repository,
        ILeadConversionOrchestrator orchestrator,
        IEventBus eventBus)
    {
        _repository = repository;
        _orchestrator = orchestrator;
        _eventBus = eventBus;
    }

    public async ValueTask<ConvertLeadResult> HandleAsync(
        ConvertLeadCommand<TConvertRequest> command, CancellationToken ct = default)
    {
        var lead = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new LeadValidationException($"Lead '{command.Id}' not found");

        if (lead.StatusId != LeadWellKnownIds.StatusQualified)
            throw new LeadValidationException("Only a qualified lead can be converted.");

        var request = new LeadConversionRequest(
            lead.Id, lead.FullName, lead.Email?.Value, lead.Phone?.Value, lead.Company,
            command.Request.CreateDeal, command.Request.DealTitle, command.Request.Amount,
            command.Request.Currency, command.Request.PipelineId);

        var outcome = await _orchestrator.ConvertAsync(request, ct);

        lead.MarkConverted(outcome.CustomerId, outcome.DealId);
        await _repository.SaveChangesAsync(ct);

        foreach (var e in lead.DomainEvents)
            await _eventBus.PublishAsync(e, ct);
        lead.ClearDomainEvents();

        return new ConvertLeadResult(outcome.CustomerId, outcome.DealId);
    }
}
