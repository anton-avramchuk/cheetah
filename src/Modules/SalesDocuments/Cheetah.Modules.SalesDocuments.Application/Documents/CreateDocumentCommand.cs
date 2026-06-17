using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Modules.SalesDocuments.Application.Abstractions;
using Cheetah.Modules.SalesDocuments.Contracts;
using Cheetah.Modules.SalesDocuments.Domain.Abstractions;
using Cheetah.Modules.SalesDocuments.Domain.Entities;

namespace Cheetah.Modules.SalesDocuments.Application.Documents;

/// <summary>Создать документ-черновик из запроса наследника (вместе со строками).</summary>
public sealed record CreateDocumentCommand<TCreateRequest>(TCreateRequest Request) : ICommand<Guid>
    where TCreateRequest : CreateDocumentRequestBase;

public class CreateDocumentCommandHandler<TDoc, TCreateRequest>
    : ICommandHandler<CreateDocumentCommand<TCreateRequest>, Guid>
    where TDoc : SalesDocumentBase
    where TCreateRequest : CreateDocumentRequestBase
{
    private readonly ISalesDocumentFactory<TDoc, TCreateRequest> _factory;
    private readonly IRepository<TDoc, Guid> _repository;
    private readonly IProductPricingPort _pricing;
    private readonly IEventBus _eventBus;

    public CreateDocumentCommandHandler(
        ISalesDocumentFactory<TDoc, TCreateRequest> factory,
        IRepository<TDoc, Guid> repository,
        IProductPricingPort pricing,
        IEventBus eventBus)
    {
        _factory = factory;
        _repository = repository;
        _pricing = pricing;
        _eventBus = eventBus;
    }

    public async ValueTask<Guid> HandleAsync(CreateDocumentCommand<TCreateRequest> command, CancellationToken ct = default)
    {
        var document = _factory.Create(command.Request);
        await DocumentHandlerHelpers.ApplyLinesAsync(document, command.Request.Lines, _pricing, ct);

        _repository.Add(document);
        await DocumentHandlerHelpers.SaveAndPublishAsync(_repository, _eventBus, document, ct);

        return document.Id;
    }
}
