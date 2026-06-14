using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Modules.Customer.Application.Abstractions;
using Cheetah.Modules.Customer.Contracts;
using Cheetah.Modules.Customer.Domain.Entities;

namespace Cheetah.Modules.Customer.Application.Customers;

/// <summary>Получить клиента по идентификатору (null, если не найден).</summary>
public sealed record GetCustomerByIdQuery<TDto>(Guid Id) : IQuery<TDto?>
    where TDto : CustomerDtoBase;

public class GetCustomerByIdQueryHandler<TCustomer, TDto> : IQueryHandler<GetCustomerByIdQuery<TDto>, TDto?>
    where TCustomer : CustomerBase
    where TDto : CustomerDtoBase
{
    private readonly IRepository<TCustomer, Guid> _repository;
    private readonly ICustomerProjector<TCustomer, TDto> _projector;

    public GetCustomerByIdQueryHandler(IRepository<TCustomer, Guid> repository, ICustomerProjector<TCustomer, TDto> projector)
    {
        _repository = repository;
        _projector = projector;
    }

    public async ValueTask<TDto?> HandleAsync(GetCustomerByIdQuery<TDto> query, CancellationToken ct = default)
    {
        var customer = await _repository.GetByIdAsync(query.Id, ct);
        return customer is null ? null : _projector.ToDto(customer);
    }
}
