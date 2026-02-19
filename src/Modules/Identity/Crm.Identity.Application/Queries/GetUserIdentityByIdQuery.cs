using Cheetah.Core.CQRS;

namespace Crm.Identity.Application.Queries;

public record GetUserIdentityByIdQuery(Guid Id) : IQuery<UserIdentityModel?>;