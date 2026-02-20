using Cheetah.Core.CQRS;

namespace Crm.Identity.Application.Queries;

public record GetRoleByIdQuery(Guid Id) : IQuery<RoleModel?>;
