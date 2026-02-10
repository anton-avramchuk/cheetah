using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Queries;

public record GetAllCustomersQuery : IQuery<IReadOnlyList<CustomerModel>>;
