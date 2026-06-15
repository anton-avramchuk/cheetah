using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Core.StateMachine;
using Cheetah.Modules.Leads.Application.Exceptions;
using Cheetah.Modules.Leads.Contracts;
using Cheetah.Modules.Leads.Domain.Entities;
using Cheetah.Modules.Leads.Shared;

namespace Cheetah.Modules.Leads.Application.Leads;

// ── Обновление ────────────────────────────────────────────────────────────────────────────────

/// <summary>Обновить базовые поля лида (имя, компания, контакты). Скоринг пересчитывается.</summary>
public sealed record UpdateLeadCommand<TUpdateRequest>(Guid Id, TUpdateRequest Request) : ICommand
    where TUpdateRequest : UpdateLeadRequestBase;

public class UpdateLeadCommandHandler<TLead, TUpdateRequest> : ICommandHandler<UpdateLeadCommand<TUpdateRequest>>
    where TLead : LeadBase
    where TUpdateRequest : UpdateLeadRequestBase
{
    private readonly IRepository<TLead, Guid> _repository;

    public UpdateLeadCommandHandler(IRepository<TLead, Guid> repository) => _repository = repository;

    public async ValueTask HandleAsync(UpdateLeadCommand<TUpdateRequest> command, CancellationToken ct = default)
    {
        var lead = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new LeadValidationException($"Lead '{command.Id}' not found");

        lead.Update(command.Request.FullName, command.Request.Company, command.Request.Email, command.Request.Phone);
        await _repository.SaveChangesAsync(ct);
    }
}

// ── Квалификация ─────────────────────────────────────────────────────────────────────────────

/// <summary>Квалифицировать лид.</summary>
public sealed record QualifyLeadCommand(Guid Id) : ICommand;

public class QualifyLeadCommandHandler<TLead> : ICommandHandler<QualifyLeadCommand>
    where TLead : LeadBase
{
    private readonly IRepository<TLead, Guid> _repository;
    private readonly IEventBus _eventBus;
    private readonly IStateMachineValidator<LeadStatus> _stateMachine;

    public QualifyLeadCommandHandler(
        IRepository<TLead, Guid> repository, IEventBus eventBus, IStateMachineValidator<LeadStatus> stateMachine)
    {
        _repository = repository;
        _eventBus = eventBus;
        _stateMachine = stateMachine;
    }

    public async ValueTask HandleAsync(QualifyLeadCommand command, CancellationToken ct = default)
    {
        var lead = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new LeadValidationException($"Lead '{command.Id}' not found");

        _stateMachine.ValidateTransition(lead.Status, LeadStatus.Qualified);
        lead.Qualify();
        await SaveAndPublishAsync(_repository, _eventBus, lead, ct);
    }

    internal static async ValueTask SaveAndPublishAsync(
        IRepository<TLead, Guid> repository, IEventBus eventBus, TLead lead, CancellationToken ct)
    {
        await repository.SaveChangesAsync(ct);
        foreach (var e in lead.DomainEvents)
            await eventBus.PublishAsync(e, ct);
        lead.ClearDomainEvents();
    }
}

// ── Дисквалификация ──────────────────────────────────────────────────────────────────────────

/// <summary>Дисквалифицировать лид (требуется причина).</summary>
public sealed record DisqualifyLeadCommand(Guid Id, string Reason) : ICommand;

public class DisqualifyLeadCommandHandler<TLead> : ICommandHandler<DisqualifyLeadCommand>
    where TLead : LeadBase
{
    private readonly IRepository<TLead, Guid> _repository;
    private readonly IEventBus _eventBus;
    private readonly IStateMachineValidator<LeadStatus> _stateMachine;

    public DisqualifyLeadCommandHandler(
        IRepository<TLead, Guid> repository, IEventBus eventBus, IStateMachineValidator<LeadStatus> stateMachine)
    {
        _repository = repository;
        _eventBus = eventBus;
        _stateMachine = stateMachine;
    }

    public async ValueTask HandleAsync(DisqualifyLeadCommand command, CancellationToken ct = default)
    {
        var lead = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new LeadValidationException($"Lead '{command.Id}' not found");

        _stateMachine.ValidateTransition(lead.Status, LeadStatus.Disqualified);
        lead.Disqualify(command.Reason);
        await QualifyLeadCommandHandler<TLead>.SaveAndPublishAsync(_repository, _eventBus, lead, ct);
    }
}
