using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.Identity.Domain.Repositories;
using Cheetah.Identity.Contracts.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Identity.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllRolesQuery, List<RoleViewModel>>))]
public class GetAllRolesQueryHandler : IQueryHandler<GetAllRolesQuery, List<RoleViewModel>>
{
    private readonly IRoleRepository _roleRepository;

    public GetAllRolesQueryHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async ValueTask<List<RoleViewModel>> HandleAsync(GetAllRolesQuery query, CancellationToken ct)
    {
        return await _roleRepository.AsNoTrackingQueryable()
            .Select(r => new RoleViewModel
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(ct);
    }
}
