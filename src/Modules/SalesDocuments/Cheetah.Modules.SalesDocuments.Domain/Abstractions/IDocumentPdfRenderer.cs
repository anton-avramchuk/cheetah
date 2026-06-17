using Cheetah.Modules.SalesDocuments.Domain.Entities;

namespace Cheetah.Modules.SalesDocuments.Domain.Abstractions;

/// <summary>
/// Порт генерации PDF документа и его сохранения. Объявлен в Domain (исходящий порт), реализуется в
/// Infrastructure: рендерит документ (шаблон) и сохраняет через FileStorage, возвращая идентификатор
/// файла (<c>PdfFileId</c>). Так Application не зависит от FileStorage напрямую.
/// </summary>
public interface IDocumentPdfService
{
    ValueTask<Guid> GenerateAndStoreAsync(SalesDocumentBase document, CancellationToken ct = default);
}
