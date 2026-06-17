using Cheetah.Modules.SalesDocuments.Contracts;
using Cheetah.Modules.SalesDocuments.Domain.Entities;

namespace Cheetah.Modules.SalesDocuments.Application.Abstractions;

/// <summary>
/// Проектор конкретного документа в его ViewModel. Реализуется наследником — он знает свои доп. поля.
/// Так generic query-handler возвращает DTO, не зная конкретного типа.
/// </summary>
public interface ISalesDocumentProjector<in TDoc, out TDto>
    where TDoc : SalesDocumentBase
    where TDto : SalesDocumentDtoBase
{
    TDto ToDto(TDoc document);
}
