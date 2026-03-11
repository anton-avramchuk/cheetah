using Cheetah.Core.CQRS;

namespace Crm.Customer.Application.Queries;

public record GetCustomerByIdQuery(Guid Id) : IQuery<CustomerModel?>;