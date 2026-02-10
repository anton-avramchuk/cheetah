using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Queries;

public record GetCustomerByIdQuery(Guid Id) : IQuery<CustomerModel?>;
