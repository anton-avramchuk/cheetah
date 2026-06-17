using System.Text;
using Cheetah.FileStorage;
using Cheetah.Modules.SalesDocuments.Domain.Abstractions;
using Cheetah.Modules.SalesDocuments.Domain.Entities;

namespace Cheetah.Modules.SalesDocuments.Infrastructure.Services;

/// <summary>
/// Реализация PDF-порта поверх <see cref="IFileStorage"/>. MVP: рендерит документ простым текстовым
/// представлением (плейсхолдер вместо реального PDF-шаблонизатора — follow-up, план §1.2 п.4),
/// сохраняет под ключом <c>sales-documents/{documentId}/{fileId}.pdf</c> и возвращает <c>fileId</c>.
/// </summary>
public sealed class FileStorageDocumentPdfService : IDocumentPdfService
{
    private readonly IFileStorage _storage;

    public FileStorageDocumentPdfService(IFileStorage storage) => _storage = storage;

    public async ValueTask<Guid> GenerateAndStoreAsync(SalesDocumentBase document, CancellationToken ct = default)
    {
        var fileId = Guid.NewGuid();
        var key = $"sales-documents/{document.Id}/{fileId}.pdf";

        var content = Render(document);
        using var stream = new MemoryStream(content);
        await _storage.SaveAsync(key, stream, "application/pdf", cancellationToken: ct);

        return fileId;
    }

    private static byte[] Render(SalesDocumentBase document)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{document.DocType} {document.Number}");
        sb.AppendLine($"Customer: {document.CustomerId}");
        sb.AppendLine($"Currency: {document.Currency}");
        sb.AppendLine("Lines:");
        foreach (var line in document.Lines)
            sb.AppendLine($"  {line.Name} x{line.Qty} @ {line.UnitPrice} = {line.LineTotal}");
        sb.AppendLine($"Subtotal: {document.Subtotal}");
        sb.AppendLine($"Discount: {document.DiscountTotal}");
        sb.AppendLine($"Tax: {document.TaxTotal}");
        sb.AppendLine($"Total: {document.GrandTotal} {document.Currency}");
        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}
