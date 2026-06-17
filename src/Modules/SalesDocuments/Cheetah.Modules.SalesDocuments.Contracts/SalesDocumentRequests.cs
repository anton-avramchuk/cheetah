using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Cheetah.Modules.SalesDocuments.Shared;

namespace Cheetah.Modules.SalesDocuments.Contracts;

/// <summary>
/// Базовый запрос на создание документа. Абстрактен: наследник объявляет конкретный
/// <c>sealed record CreateDocumentRequest : CreateDocumentRequestBase</c>, добавляет свои поля и
/// (опционально) атрибут <c>[ApiRoute(..., ApiMethod.Create)]</c>.
/// </summary>
public abstract record CreateDocumentRequestBase : ICrmRequest
{
    public DocType DocType { get; init; }
    public Guid CustomerId { get; init; }
    public Guid? DealId { get; init; }
    public Guid OwnerId { get; init; }
    public string Currency { get; init; } = null!;
    public DateTimeOffset? ValidUntil { get; init; }
    public IReadOnlyList<AddLineRequest> Lines { get; init; } = [];
}

/// <summary>Базовый запрос на обновление шапки документа (Id — из маршрута).</summary>
public abstract record UpdateDocumentRequestBase : ICrmRequest
{
    [FromRoute] public Guid Id { get; init; }
    public Guid? DealId { get; init; }
    public DateTimeOffset? ValidUntil { get; init; }
}

/// <summary>Запрос «документ по Id».</summary>
public abstract record GetDocumentByIdRequestBase : ICrmRequest
{
    [FromRoute] public Guid Id { get; init; }
}

/// <summary>Запрос «удалить (аннулировать) документ».</summary>
public abstract record DeleteDocumentRequestBase : ICrmRequest
{
    [FromRoute] public Guid Id { get; init; }
}

/// <summary>Запрос грида документов (пагинация/сортировка/фильтрация). Наследник — конкретный class.</summary>
public abstract class GetDocumentsGridRequestBase : GridRequest;
