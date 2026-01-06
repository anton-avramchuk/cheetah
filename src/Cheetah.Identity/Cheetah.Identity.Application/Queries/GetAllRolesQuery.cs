using Cheetah.Core.CQRS;
using Cheetah.Identity.Contracts.ViewModels;

namespace Cheetah.Identity.Application.Queries;

/// <summary>
/// Query to get all roles
/// </summary>
public record GetAllRolesQuery : IQuery<List<RoleViewModel>>;
