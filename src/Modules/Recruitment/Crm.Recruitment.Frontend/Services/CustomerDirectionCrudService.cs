using Cheetah.Blazor.Components.Crud;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.ApiClient;
using Crm.Recruitment.Contracts.Requests;
using Crm.Recruitment.Frontend.Models;

namespace Crm.Recruitment.Frontend.Services;

[Export(LifetimeType.Scoped, typeof(ICrudService<CustomerDirectionGridViewModel, CustomerDirectionFormModel, CustomerDirectionFormModel>))]
public sealed class CustomerDirectionCrudService : ICrudService<CustomerDirectionGridViewModel, CustomerDirectionFormModel, CustomerDirectionFormModel>
{
    private readonly ICustomerDirectionsService _service;

    public CustomerDirectionCrudService(ICustomerDirectionsService service)
    {
        _service = service;
    }

    public async Task<GridResult<CustomerDirectionGridViewModel>> GetAllAsync(GridRequest request, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(ct);
        var mapped = result.Select(CustomerDirectionGridViewModel.FromResponse).ToList();
        return new GridResult<CustomerDirectionGridViewModel>(mapped, mapped.Count);
    }

    public async Task<CustomerDirectionFormModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _service.GetByIdAsync(id, ct);
        if (entity == null)
            return null;

        return new CustomerDirectionFormModel
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description
        };
    }

    public async Task<Guid> CreateAsync(CustomerDirectionFormModel model, CancellationToken ct = default)
    {
        var request = new CreateCustomerDirectionRequest(model.Name, model.Description);
        return await _service.CreateAsync(request, ct);
    }

    public async Task UpdateAsync(Guid id, CustomerDirectionFormModel model, CancellationToken ct = default)
    {
        var request = new UpdateCustomerDirectionRequest(id, model.Name, model.Description);
        await _service.UpdateAsync(id, request, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _service.DeleteAsync(id, ct);
    }
}
