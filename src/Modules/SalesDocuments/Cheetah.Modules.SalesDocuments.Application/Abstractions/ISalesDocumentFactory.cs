using Cheetah.Modules.SalesDocuments.Contracts;
using Cheetah.Modules.SalesDocuments.Domain.Entities;
using Cheetah.Modules.SalesDocuments.Shared;

namespace Cheetah.Modules.SalesDocuments.Application.Abstractions;

/// <summary>
/// Фабрика конкретного документа из запроса на создание. Реализуется наследником — он знает, как
/// сконструировать свою сущность (включая доп. поля) и завести инварианты/событие через
/// <c>InitializeCore</c>. Так generic-handler создаёт документ, не зная конкретного типа.
/// </summary>
public interface ISalesDocumentFactory<out TDoc, in TCreateRequest>
    where TDoc : SalesDocumentBase
    where TCreateRequest : CreateDocumentRequestBase
{
    TDoc Create(TCreateRequest request);

    /// <summary>
    /// Создаёт пустой черновик документа типа <paramref name="toType"/> «на основе» исходного
    /// (копирует шапку, проставляет <c>SourceDocumentId</c>). Строки копирует вызывающий хендлер
    /// через <c>AddLine</c>.
    /// </summary>
    TDoc CreateForConversion(SalesDocumentBase source, DocType toType);
}
