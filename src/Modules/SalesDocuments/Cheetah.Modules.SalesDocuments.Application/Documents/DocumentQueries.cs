using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Modules.SalesDocuments.Application.Abstractions;
using Cheetah.Modules.SalesDocuments.Contracts;
using Cheetah.Modules.SalesDocuments.Domain.Entities;
using Cheetah.Modules.SalesDocuments.Domain.Specifications;
using Cheetah.Modules.SalesDocuments.Shared;

namespace Cheetah.Modules.SalesDocuments.Application.Documents;

// ── Документ по Id ──────────────────────────────────────────────────────────────────────────────

/// <summary>Получить документ по идентификатору (null, если не найден).</summary>
public sealed record GetDocumentByIdQuery<TDto>(Guid Id) : IQuery<TDto?>
    where TDto : SalesDocumentDtoBase;

public class GetDocumentByIdQueryHandler<TDoc, TDto> : IQueryHandler<GetDocumentByIdQuery<TDto>, TDto?>
    where TDoc : SalesDocumentBase
    where TDto : SalesDocumentDtoBase
{
    private readonly IRepository<TDoc, Guid> _repository;
    private readonly ISalesDocumentProjector<TDoc, TDto> _projector;

    public GetDocumentByIdQueryHandler(IRepository<TDoc, Guid> repository, ISalesDocumentProjector<TDoc, TDto> projector)
    {
        _repository = repository;
        _projector = projector;
    }

    public async ValueTask<TDto?> HandleAsync(GetDocumentByIdQuery<TDto> query, CancellationToken ct = default)
    {
        var document = await _repository.GetByIdAsync(query.Id, ct);
        return document is null ? null : _projector.ToDto(document);
    }
}

// ── Список документов ───────────────────────────────────────────────────────────────────────────

/// <summary>Список документов с комбинированным фильтром (клиент/тип/статус).</summary>
public sealed record ListDocumentsQuery<TDto>(Guid? CustomerId, DocType? Type, DocumentStatus? Status)
    : IQuery<IReadOnlyList<TDto>>
    where TDto : SalesDocumentDtoBase;

public class ListDocumentsQueryHandler<TDoc, TDto> : IQueryHandler<ListDocumentsQuery<TDto>, IReadOnlyList<TDto>>
    where TDoc : SalesDocumentBase
    where TDto : SalesDocumentDtoBase
{
    private readonly IRepository<TDoc, Guid> _repository;
    private readonly ISalesDocumentProjector<TDoc, TDto> _projector;

    public ListDocumentsQueryHandler(IRepository<TDoc, Guid> repository, ISalesDocumentProjector<TDoc, TDto> projector)
    {
        _repository = repository;
        _projector = projector;
    }

    public async ValueTask<IReadOnlyList<TDto>> HandleAsync(ListDocumentsQuery<TDto> query, CancellationToken ct = default)
    {
        var spec = new DocumentsFilterSpecification<TDoc>(query.CustomerId, query.Type, query.Status);
        var items = await _repository.GetAllAsync(spec, ct);
        return items.Select(_projector.ToDto).ToArray();
    }
}
