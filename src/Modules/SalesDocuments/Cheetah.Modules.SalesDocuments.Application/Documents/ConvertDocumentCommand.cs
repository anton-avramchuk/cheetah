using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Modules.SalesDocuments.Application.Abstractions;
using Cheetah.Modules.SalesDocuments.Contracts;
using Cheetah.Modules.SalesDocuments.Domain.Entities;
using Cheetah.Modules.SalesDocuments.Shared;

namespace Cheetah.Modules.SalesDocuments.Application.Documents;

/// <summary>Создать новый документ «на основе» существующего (Quote→Order→Invoice), копируя строки.</summary>
public sealed record ConvertDocumentCommand(Guid DocumentId, DocType ToType) : ICommand<Guid>;

public class ConvertDocumentCommandHandler<TDoc, TCreateRequest> : ICommandHandler<ConvertDocumentCommand, Guid>
    where TDoc : SalesDocumentBase
    where TCreateRequest : CreateDocumentRequestBase
{
    private readonly IRepository<TDoc, Guid> _repository;
    private readonly ISalesDocumentFactory<TDoc, TCreateRequest> _factory;
    private readonly IEventBus _eventBus;

    public ConvertDocumentCommandHandler(
        IRepository<TDoc, Guid> repository, ISalesDocumentFactory<TDoc, TCreateRequest> factory, IEventBus eventBus)
    {
        _repository = repository;
        _factory = factory;
        _eventBus = eventBus;
    }

    public async ValueTask<Guid> HandleAsync(ConvertDocumentCommand command, CancellationToken ct = default)
    {
        var source = await DocumentHandlerHelpers.GetOrThrowAsync(_repository, command.DocumentId, ct);

        var target = _factory.CreateForConversion(source, command.ToType);
        foreach (var line in source.Lines)
            target.AddLine(line.ProductId, line.Name, line.UnitPrice, line.Qty, line.DiscountPercent, line.TaxRate);

        _repository.Add(target);
        await DocumentHandlerHelpers.SaveAndPublishAsync(_repository, _eventBus, target, ct);

        return target.Id;
    }
}
