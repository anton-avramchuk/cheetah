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

[Export(LifetimeType.Scoped,
    typeof(ICrudService<UserIdentityGridViewModel, UserIdentityFormModel, UserIdentityFormModel>))]
public sealed class
    UserIdentityCrudService : ICrudService<UserIdentityGridViewModel, UserIdentityFormModel, UserIdentityFormModel>
{
    private readonly IIdentityService _service;

    public UserIdentityCrudService(IIdentityService service)
    {
        _service = service;
    }

    public async Task<GridResult<UserIdentityGridViewModel>> GetAllAsync(GridRequest request,
        CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(null, ct);
        var mapped = result.Data.Select(UserIdentityGridViewModel.FromResponse).ToList();
        return new GridResult<UserIdentityGridViewModel>(mapped, result.Total);
    }

    public async Task<UserIdentityFormModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _service.GetByIdAsync(id, ct);
        if (entity == null)
            return null;

        return new UserIdentityFormModel
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description ?? ""
        };
    }

    public async Task<Guid> CreateAsync(UserIdentityFormModel model, CancellationToken ct = default)
    {
        var description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description;
        var request = new CreateUserIdentityRequest(model.Name, description);
        return await _service.CreateAsync(request, ct);
    }

    public async Task UpdateAsync(Guid id, UserIdentityFormModel model, CancellationToken ct = default)
    {
        var description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description;
        var request = new UpdateUserIdentityRequest(id, model.Name, description);
        await _service.UpdateAsync(id, request, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _service.DeleteAsync(id, ct);
    }
}