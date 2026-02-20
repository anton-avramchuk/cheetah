using Cheetah.Core.CQRS;

namespace Crm.Identity.Application.Queries;

public record GetAllRolesQuery : IQuery<IReadOnlyList<RoleModel>>;
