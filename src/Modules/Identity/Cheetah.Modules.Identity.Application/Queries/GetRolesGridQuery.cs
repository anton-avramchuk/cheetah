using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Modules.Identity.Application.Models;

namespace Cheetah.Modules.Identity.Application.Queries;

public record GetRolesGridQuery<TRoleModel>(
    int Page,
    int PageSize,
    List<SortDescriptor> Sort,
    FilterDescriptor? Filter
) : IQuery<GridResult<TRoleModel>>
    where TRoleModel : RoleModel;

public record GetRolesGridQuery(int Page, int PageSize, List<SortDescriptor> Sort, FilterDescriptor? Filter)
    : GetRolesGridQuery<RoleModel>(Page, PageSize, Sort, Filter);
