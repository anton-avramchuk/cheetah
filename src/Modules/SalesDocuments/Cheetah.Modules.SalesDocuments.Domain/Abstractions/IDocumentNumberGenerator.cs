using Cheetah.Modules.SalesDocuments.Shared;

namespace Cheetah.Modules.SalesDocuments.Domain.Abstractions;

/// <summary>
/// Порт генерации номера документа. Объявлен в Domain (исходящий порт, как репозитории), реализуется
/// в Infrastructure. Номер — последовательный per <see cref="DocType"/> per год, формат
/// <c>{Prefix}-{Year}-{Seq}</c>. Реализация обеспечивает отсутствие гонок (БД-секвенс/DistributedLock).
/// </summary>
public interface IDocumentNumberGenerator
{
    ValueTask<string> NextAsync(DocType docType, CancellationToken ct = default);
}
