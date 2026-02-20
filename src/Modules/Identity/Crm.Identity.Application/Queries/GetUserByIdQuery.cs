using Cheetah.Core.CQRS;

namespace Crm.Identity.Application.Queries;

public record GetUserByIdQuery(Guid Id) : IQuery<UserModel?>;
