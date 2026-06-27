using Cheetah.Core.CQRS;
using Cheetah.Modules.Identity.Application.Models;

namespace Cheetah.Modules.Identity.Application.Queries;

public record GetRoleByIdQuery<TRoleModel>(Guid Id) : IQuery<TRoleModel?>
    where TRoleModel : RoleModel;
