using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Specification;
using Cheetah.Modules.Customer.Application.Abstractions;
using Cheetah.Modules.Customer.Contracts;
using Cheetah.Modules.Customer.Domain.Entities;
using Cheetah.Modules.Customer.Domain.Specifications;

namespace Cheetah.Modules.Customer.Application.Customers;

/// <summary>Список клиентов (опц. только активные).</summary>
public sealed record ListCustomersQuery<TDto>(bool ActiveOnly = false) : IQuery<IReadOnlyList<TDto>>
    where TDto : CustomerDtoBase;

public class ListCustomersQueryHandler<TCustomer, TDto> : IQueryHandler<ListCustomersQuery<TDto>, IReadOnlyList<TDto>>
    where TCustomer : CustomerBase
    where TDto : CustomerDtoBase
{
    private readonly IRepository<TCustomer, Guid> _repository;
    private readonly ICustomerProjector<TCustomer, TDto> _projector;

    public ListCustomersQueryHandler(IRepository<TCustomer, Guid> repository, ICustomerProjector<TCustomer, TDto> projector)
    {
        _repository = repository;
        _projector = projector;
    }

    public async ValueTask<IReadOnlyList<TDto>> HandleAsync(ListCustomersQuery<TDto> query, CancellationToken ct = default)
    {
        ISpecification<TCustomer>? spec = query.ActiveOnly
            ? new ActiveCustomersSpecification<TCustomer>()
            : null;

        var items = await _repository.GetAllAsync(spec, ct);
        return items.Select(_projector.ToDto).ToArray();
    }
}
