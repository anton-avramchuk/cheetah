using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Modules.Identity.Application.Models;

namespace Cheetah.Modules.Identity.Application.Queries;

public record GetAllRolesQuery(
    int Page,
    int PageSize,
    List<SortDescriptor> Sort,
    FilterDescriptor? Filter
) : IQuery<GridResult<RoleModel>>;
