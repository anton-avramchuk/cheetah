using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Modules.Customer.Application.Abstractions;
using Cheetah.Modules.Customer.Contracts;
using Cheetah.Modules.Customer.Domain.Entities;
using Cheetah.Modules.Customer.Domain.Specifications;

namespace Cheetah.Modules.Customer.Application.Contacts;

/// <summary>Список контактных лиц клиента (по умолчанию — без удалённых).</summary>
public sealed record ListContactsByCustomerQuery<TDto>(Guid CustomerId, bool IncludeRemoved = false)
    : IQuery<IReadOnlyList<TDto>>
    where TDto : ContactDtoBase;

public class ListContactsByCustomerQueryHandler<TContact, TDto>
    : IQueryHandler<ListContactsByCustomerQuery<TDto>, IReadOnlyList<TDto>>
    where TContact : ContactBase
    where TDto : ContactDtoBase
{
    private readonly IRepository<TContact, Guid> _repository;
    private readonly IContactProjector<TContact, TDto> _projector;

    public ListContactsByCustomerQueryHandler(IRepository<TContact, Guid> repository, IContactProjector<TContact, TDto> projector)
    {
        _repository = repository;
        _projector = projector;
    }

    public async ValueTask<IReadOnlyList<TDto>> HandleAsync(ListContactsByCustomerQuery<TDto> query, CancellationToken ct = default)
    {
        var spec = new ContactsByCustomerSpecification<TContact>(query.CustomerId, query.IncludeRemoved);
        var items = await _repository.GetAllAsync(spec, ct);
        return items.Select(_projector.ToDto).ToArray();
    }
}
