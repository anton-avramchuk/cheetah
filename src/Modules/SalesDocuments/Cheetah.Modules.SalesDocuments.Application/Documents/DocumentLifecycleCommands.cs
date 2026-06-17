using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Core.StateMachine;
using Cheetah.Modules.SalesDocuments.Domain.Abstractions;
using Cheetah.Modules.SalesDocuments.Domain.Entities;
using Cheetah.Modules.SalesDocuments.Shared;

namespace Cheetah.Modules.SalesDocuments.Application.Documents;

// ── Выпуск (присвоить номер) ───────────────────────────────────────────────────────────────────

/// <summary>Выпустить документ из черновика: присвоить номер и перевести в выпущенный статус.</summary>
public sealed record IssueDocumentCommand(Guid DocumentId) : ICommand;

public class IssueDocumentCommandHandler<TDoc> : ICommandHandler<IssueDocumentCommand>
    where TDoc : SalesDocumentBase
{
    private readonly IRepository<TDoc, Guid> _repository;
    private readonly IDocumentNumberGenerator _numbers;
    private readonly IStateMachineValidator<DocumentStatus> _stateMachine;
    private readonly IEventBus _eventBus;

    public IssueDocumentCommandHandler(
        IRepository<TDoc, Guid> repository, IDocumentNumberGenerator numbers,
        IStateMachineValidator<DocumentStatus> stateMachine, IEventBus eventBus)
    {
        _repository = repository;
        _numbers = numbers;
        _stateMachine = stateMachine;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(IssueDocumentCommand command, CancellationToken ct = default)
    {
        var document = await DocumentHandlerHelpers.GetOrThrowAsync(_repository, command.DocumentId, ct);

        var target = document.DocType switch
        {
            DocType.Quote => DocumentStatus.Sent,
            DocType.Order => DocumentStatus.Confirmed,
            _ => DocumentStatus.Issued
        };
        _stateMachine.ValidateTransition(document.Status, target);

        var number = await _numbers.NextAsync(document.DocType, ct);
        document.Issue(number);

        await DocumentHandlerHelpers.SaveAndPublishAsync(_repository, _eventBus, document, ct);
    }
}

// ── Принять КП ────────────────────────────────────────────────────────────────────────────────

/// <summary>Принять КП клиентом (Sent → Accepted).</summary>
public sealed record AcceptQuoteCommand(Guid DocumentId) : ICommand;

public class AcceptQuoteCommandHandler<TDoc> : ICommandHandler<AcceptQuoteCommand>
    where TDoc : SalesDocumentBase
{
    private readonly IRepository<TDoc, Guid> _repository;
    private readonly IStateMachineValidator<DocumentStatus> _stateMachine;
    private readonly IEventBus _eventBus;

    public AcceptQuoteCommandHandler(
        IRepository<TDoc, Guid> repository, IStateMachineValidator<DocumentStatus> stateMachine, IEventBus eventBus)
    {
        _repository = repository;
        _stateMachine = stateMachine;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(AcceptQuoteCommand command, CancellationToken ct = default)
    {
        var document = await DocumentHandlerHelpers.GetOrThrowAsync(_repository, command.DocumentId, ct);
        _stateMachine.ValidateTransition(document.Status, DocumentStatus.Accepted);
        document.Accept();
        await DocumentHandlerHelpers.SaveAndPublishAsync(_repository, _eventBus, document, ct);
    }
}

// ── Отклонить КП ──────────────────────────────────────────────────────────────────────────────

/// <summary>Отклонить КП клиентом (Sent → Rejected).</summary>
public sealed record RejectQuoteCommand(Guid DocumentId, string? Reason) : ICommand;

public class RejectQuoteCommandHandler<TDoc> : ICommandHandler<RejectQuoteCommand>
    where TDoc : SalesDocumentBase
{
    private readonly IRepository<TDoc, Guid> _repository;
    private readonly IStateMachineValidator<DocumentStatus> _stateMachine;
    private readonly IEventBus _eventBus;

    public RejectQuoteCommandHandler(
        IRepository<TDoc, Guid> repository, IStateMachineValidator<DocumentStatus> stateMachine, IEventBus eventBus)
    {
        _repository = repository;
        _stateMachine = stateMachine;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(RejectQuoteCommand command, CancellationToken ct = default)
    {
        var document = await DocumentHandlerHelpers.GetOrThrowAsync(_repository, command.DocumentId, ct);
        _stateMachine.ValidateTransition(document.Status, DocumentStatus.Rejected);
        document.Reject(command.Reason);
        await DocumentHandlerHelpers.SaveAndPublishAsync(_repository, _eventBus, document, ct);
    }
}

// ── Оплата счёта ──────────────────────────────────────────────────────────────────────────────

/// <summary>Отметить счёт оплаченным (Issued/Overdue → Paid).</summary>
public sealed record MarkInvoicePaidCommand(Guid DocumentId) : ICommand;

public class MarkInvoicePaidCommandHandler<TDoc> : ICommandHandler<MarkInvoicePaidCommand>
    where TDoc : SalesDocumentBase
{
    private readonly IRepository<TDoc, Guid> _repository;
    private readonly IStateMachineValidator<DocumentStatus> _stateMachine;
    private readonly IEventBus _eventBus;

    public MarkInvoicePaidCommandHandler(
        IRepository<TDoc, Guid> repository, IStateMachineValidator<DocumentStatus> stateMachine, IEventBus eventBus)
    {
        _repository = repository;
        _stateMachine = stateMachine;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(MarkInvoicePaidCommand command, CancellationToken ct = default)
    {
        var document = await DocumentHandlerHelpers.GetOrThrowAsync(_repository, command.DocumentId, ct);
        _stateMachine.ValidateTransition(document.Status, DocumentStatus.Paid);
        document.MarkPaid();
        await DocumentHandlerHelpers.SaveAndPublishAsync(_repository, _eventBus, document, ct);
    }
}

// ── Аннулирование ─────────────────────────────────────────────────────────────────────────────

/// <summary>Аннулировать документ.</summary>
public sealed record CancelDocumentCommand(Guid DocumentId, string? Reason) : ICommand;

public class CancelDocumentCommandHandler<TDoc> : ICommandHandler<CancelDocumentCommand>
    where TDoc : SalesDocumentBase
{
    private readonly IRepository<TDoc, Guid> _repository;
    private readonly IEventBus _eventBus;

    public CancelDocumentCommandHandler(IRepository<TDoc, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(CancelDocumentCommand command, CancellationToken ct = default)
    {
        var document = await DocumentHandlerHelpers.GetOrThrowAsync(_repository, command.DocumentId, ct);
        document.Cancel(command.Reason); // доменный guard: запрещено для Paid/Cancelled
        await DocumentHandlerHelpers.SaveAndPublishAsync(_repository, _eventBus, document, ct);
    }
}
