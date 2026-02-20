using Cheetah.Core.CQRS;

namespace Crm.Identity.Application.Queries;

public record GetAllUsersQuery : IQuery<IReadOnlyList<UserModel>>;
