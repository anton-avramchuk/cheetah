using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Modules.SalesDocuments.Domain.Abstractions;
using Cheetah.Modules.SalesDocuments.Domain.Entities;

namespace Cheetah.Modules.SalesDocuments.Application.Documents;

/// <summary>Сгенерировать PDF документа, сохранить в FileStorage и привязать к документу (PdfFileId).</summary>
public sealed record GenerateDocumentPdfCommand(Guid DocumentId) : ICommand<Guid>;

public class GenerateDocumentPdfCommandHandler<TDoc> : ICommandHandler<GenerateDocumentPdfCommand, Guid>
    where TDoc : SalesDocumentBase
{
    private readonly IRepository<TDoc, Guid> _repository;
    private readonly IDocumentPdfService _pdf;
    private readonly IEventBus _eventBus;

    public GenerateDocumentPdfCommandHandler(
        IRepository<TDoc, Guid> repository, IDocumentPdfService pdf, IEventBus eventBus)
    {
        _repository = repository;
        _pdf = pdf;
        _eventBus = eventBus;
    }

    public async ValueTask<Guid> HandleAsync(GenerateDocumentPdfCommand command, CancellationToken ct = default)
    {
        var document = await DocumentHandlerHelpers.GetOrThrowAsync(_repository, command.DocumentId, ct);

        var fileId = await _pdf.GenerateAndStoreAsync(document, ct);
        document.AttachPdf(fileId);

        await DocumentHandlerHelpers.SaveAndPublishAsync(_repository, _eventBus, document, ct);
        return fileId;
    }
}
