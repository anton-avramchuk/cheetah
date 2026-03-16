using Cheetah.Core.CQRS;
using Cheetah.Modules.Identity.Application.Models;

namespace Cheetah.Modules.Identity.Application.Queries;

public record GetRoleByIdQuery(Guid Id) : IQuery<RoleModel?>;
