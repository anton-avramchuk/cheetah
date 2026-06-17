using Cheetah.Modules.SalesDocuments.Domain.Abstractions;
using Cheetah.Modules.SalesDocuments.Domain.Entities;
using Cheetah.Modules.SalesDocuments.Infrastructure.Persistence;
using Cheetah.Modules.SalesDocuments.Shared;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.SalesDocuments.Infrastructure.Services;

/// <summary>
/// Простой последовательный генератор номеров per <see cref="DocType"/> per год:
/// <c>{Prefix}-{Year}-{Seq:000000}</c>. Считает уже существующие документы текущего года данного типа.
/// MVP-реализация: устойчивость к гонкам обеспечивается уникальным индексом <c>(DocType, Number)</c>
/// на уровне БД; полноценный без-дырочный секвенс/DistributedLock — follow-up (см. план §1.2).
/// </summary>
public sealed class SequentialDocumentNumberGenerator<TContext, TDoc> : IDocumentNumberGenerator
    where TContext : SalesDocumentsDbContextBase<TContext, TDoc>
    where TDoc : SalesDocumentBase
{
    private readonly TContext _context;

    public SequentialDocumentNumberGenerator(TContext context) => _context = context;

    public async ValueTask<string> NextAsync(DocType docType, CancellationToken ct = default)
    {
        var prefix = docType switch
        {
            DocType.Quote => "QUO",
            DocType.Order => "ORD",
            DocType.Invoice => "INV",
            _ => "DOC"
        };
        var year = DateTimeOffset.UtcNow.Year;
        var marker = $"{prefix}-{year}-";

        var count = await _context.Documents
            .CountAsync(d => d.DocType == docType && d.Number.StartsWith(marker), ct);

        return $"{marker}{count + 1:000000}";
    }
}
