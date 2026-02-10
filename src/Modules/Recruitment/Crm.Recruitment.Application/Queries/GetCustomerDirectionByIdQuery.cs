using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Queries;

public record GetCustomerDirectionByIdQuery(Guid Id) : IQuery<CustomerDirectionModel?>;
