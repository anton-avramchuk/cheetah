using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Modules.SalesDocuments.Contracts;
using Cheetah.Modules.SalesDocuments.Domain.Abstractions;
using Cheetah.Modules.SalesDocuments.Domain.Entities;

namespace Cheetah.Modules.SalesDocuments.Application.Documents;

// ── Добавить строку ───────────────────────────────────────────────────────────────────────────

/// <summary>Добавить строку в черновик документа (цена — из каталога или явная).</summary>
public sealed record AddLineCommand(Guid DocumentId, AddLineRequest Line) : ICommand;

public class AddLineCommandHandler<TDoc> : ICommandHandler<AddLineCommand>
    where TDoc : SalesDocumentBase
{
    private readonly IRepository<TDoc, Guid> _repository;
    private readonly IProductPricingPort _pricing;
    private readonly IEventBus _eventBus;

    public AddLineCommandHandler(IRepository<TDoc, Guid> repository, IProductPricingPort pricing, IEventBus eventBus)
    {
        _repository = repository;
        _pricing = pricing;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(AddLineCommand command, CancellationToken ct = default)
    {
        var document = await DocumentHandlerHelpers.GetOrThrowAsync(_repository, command.DocumentId, ct);
        await DocumentHandlerHelpers.AddResolvedLineAsync(document, command.Line, _pricing, ct);
        await DocumentHandlerHelpers.SaveAndPublishAsync(_repository, _eventBus, document, ct);
    }
}

// ── Удалить строку ────────────────────────────────────────────────────────────────────────────

/// <summary>Удалить строку из черновика документа.</summary>
public sealed record RemoveLineCommand(Guid DocumentId, Guid LineId) : ICommand;

public class RemoveLineCommandHandler<TDoc> : ICommandHandler<RemoveLineCommand>
    where TDoc : SalesDocumentBase
{
    private readonly IRepository<TDoc, Guid> _repository;
    private readonly IEventBus _eventBus;

    public RemoveLineCommandHandler(IRepository<TDoc, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(RemoveLineCommand command, CancellationToken ct = default)
    {
        var document = await DocumentHandlerHelpers.GetOrThrowAsync(_repository, command.DocumentId, ct);
        document.RemoveLine(command.LineId);
        await DocumentHandlerHelpers.SaveAndPublishAsync(_repository, _eventBus, document, ct);
    }
}

// ── Обновить шапку ────────────────────────────────────────────────────────────────────────────

/// <summary>Обновить шапку черновика документа (сделка, срок действия).</summary>
public sealed record UpdateDocumentCommand<TUpdateRequest>(Guid Id, TUpdateRequest Request) : ICommand
    where TUpdateRequest : UpdateDocumentRequestBase;

public class UpdateDocumentCommandHandler<TDoc, TUpdateRequest> : ICommandHandler<UpdateDocumentCommand<TUpdateRequest>>
    where TDoc : SalesDocumentBase
    where TUpdateRequest : UpdateDocumentRequestBase
{
    private readonly IRepository<TDoc, Guid> _repository;
    private readonly IEventBus _eventBus;

    public UpdateDocumentCommandHandler(IRepository<TDoc, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(UpdateDocumentCommand<TUpdateRequest> command, CancellationToken ct = default)
    {
        var document = await DocumentHandlerHelpers.GetOrThrowAsync(_repository, command.Id, ct);
        document.UpdateHeader(command.Request.DealId, command.Request.ValidUntil);
        await DocumentHandlerHelpers.SaveAndPublishAsync(_repository, _eventBus, document, ct);
    }
}
