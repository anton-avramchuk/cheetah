using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cheetah.Blazor.Components.Crud;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.DependencyInjection;
using Crm.Identity.ApiClient;
using Crm.Identity.Contracts.Requests;
using Crm.Identity.Frontend.Models;

namespace Crm.Identity.Frontend.Services;

[Export(LifetimeType.Scoped, typeof(ICrudService<RoleGridViewModel, RoleFormModel, RoleFormModel>))]
public sealed class RoleCrudService : ICrudService<RoleGridViewModel, RoleFormModel, RoleFormModel>
{
    private readonly IRolesService _service;

    public RoleCrudService(IRolesService service)
    {
        _service = service;
    }

    public async Task<GridResult<RoleGridViewModel>> GetAllAsync(GridRequest request, CancellationToken ct = default)
    {
        // TODO: передавать пагинацию/фильтрацию в API, когда бэкенд добавит поддержку GridRequest.
        var result = await _service.GetAllAsync(ct);
        var mapped = result.Select(RoleGridViewModel.FromResponse).ToList();
        return new GridResult<RoleGridViewModel>(mapped, mapped.Count);
    }

    public async Task<RoleFormModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _service.GetByIdAsync(id, ct);
        if (entity == null)
            return null;

        return new RoleFormModel
        {
            Id = entity.Id,
            Name = entity.Name
        };
    }

    public async Task<Guid> CreateAsync(RoleFormModel model, CancellationToken ct = default)
    {
        var request = new CreateRoleRequest(model.Name);
        return await _service.CreateAsync(request, ct);
    }

    public async Task UpdateAsync(Guid id, RoleFormModel model, CancellationToken ct = default)
    {
        var request = new UpdateRoleRequest(id, model.Name);
        await _service.UpdateAsync(id, request, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _service.DeleteAsync(id, ct);
    }
}
