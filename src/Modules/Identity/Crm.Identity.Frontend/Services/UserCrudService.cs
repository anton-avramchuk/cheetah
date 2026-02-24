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

[Export(LifetimeType.Scoped, typeof(ICrudService<UserGridViewModel, UserFormModel, UserFormModel>))]
public sealed class UserCrudService : ICrudService<UserGridViewModel, UserFormModel, UserFormModel>
{
    private readonly IUsersService _service;

    public UserCrudService(IUsersService service)
    {
        _service = service;
    }

    public async Task<GridResult<UserGridViewModel>> GetAllAsync(GridRequest request, CancellationToken ct = default)
    {
        // TODO: передавать пагинацию/фильтрацию в API, когда бэкенд добавит поддержку GridRequest.
        var result = await _service.GetAllAsync(ct);
        var mapped = result.Select(UserGridViewModel.FromResponse).ToList();
        return new GridResult<UserGridViewModel>(mapped, mapped.Count);
    }

    public async Task<UserFormModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _service.GetByIdAsync(id, ct);
        if (entity == null)
            return null;

        return new UserFormModel
        {
            Id = entity.Id,
            UserName = entity.UserName,
            Email = entity.Email
        };
    }

    public async Task<Guid> CreateAsync(UserFormModel model, CancellationToken ct = default)
    {
        var request = new CreateUserRequest(model.UserName, model.Email, model.Password ?? "");
        return await _service.CreateAsync(request, ct);
    }

    public async Task UpdateAsync(Guid id, UserFormModel model, CancellationToken ct = default)
    {
        var request = new UpdateUserRequest(id, model.UserName, model.Email);
        await _service.UpdateAsync(id, request, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _service.DeleteAsync(id, ct);
    }
}
