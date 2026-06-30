using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Modules.Customer.Application.Abstractions;
using Cheetah.Modules.Customer.Contracts;
using Cheetah.Modules.Customer.Domain.Entities;

namespace Cheetah.Modules.Customer.Application.Contacts;

/// <summary>Получить контактное лицо по идентификатору (null, если не найдено).</summary>
public sealed record GetContactByIdQuery<TDto>(Guid Id) : IQuery<TDto?>
    where TDto : ContactDtoBase;

public class GetContactByIdQueryHandler<TContact, TDto, TPosition> : IQueryHandler<GetContactByIdQuery<TDto>, TDto?>
    where TContact : ContactBase<TPosition>
    where TDto : ContactDtoBase
    where TPosition : PositionBase
{
    private readonly IRepository<TContact, Guid> _repository;
    private readonly IContactProjector<TContact, TDto, TPosition> _projector;

    public GetContactByIdQueryHandler(IRepository<TContact, Guid> repository, IContactProjector<TContact, TDto, TPosition> projector)
    {
        _repository = repository;
        _projector = projector;
    }

    public async ValueTask<TDto?> HandleAsync(GetContactByIdQuery<TDto> query, CancellationToken ct = default)
    {
        var contact = await _repository.GetByIdAsync(query.Id, ct);
        return contact is null ? null : _projector.ToDto(contact);
    }
}
