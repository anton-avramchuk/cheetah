using Cheetah.Blazor.Components.Crud;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.ApiClient;
using Crm.Recruitment.Contracts.Requests;
using Crm.Recruitment.Frontend.Models;

namespace Crm.Recruitment.Frontend.Services;

[Export(LifetimeType.Scoped, typeof(ICrudService<CustomerGridViewModel, CustomerFormModel, CustomerFormModel>))]
public sealed class CustomerCrudService : ICrudService<CustomerGridViewModel, CustomerFormModel, CustomerFormModel>
{
    private readonly ICustomersService _service;

    public CustomerCrudService(ICustomersService service)
    {
        _service = service;
    }

    public async Task<GridResult<CustomerGridViewModel>> GetAllAsync(GridRequest request, CancellationToken ct = default)
    {
        var result = await _service.GetAllAsync(ct);
        var mapped = result.Select(CustomerGridViewModel.FromResponse).ToList();
        return new GridResult<CustomerGridViewModel>(mapped, mapped.Count);
    }

    public async Task<CustomerFormModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _service.GetByIdAsync(id, ct);
        if (entity == null)
            return null;

        return new CustomerFormModel
        {
            Id = entity.Id,
            Name = entity.Name,
            Code = entity.Code,
            Description = entity.Description,
            DirectionId = entity.DirectionId
        };
    }

    public async Task<Guid> CreateAsync(CustomerFormModel model, CancellationToken ct = default)
    {
        var request = new CreateCustomerRequest(model.Name, model.Code, model.Description, model.DirectionId);
        return await _service.CreateAsync(request, ct);
    }

    public async Task UpdateAsync(Guid id, CustomerFormModel model, CancellationToken ct = default)
    {
        var request = new UpdateCustomerRequest(id, model.Name, model.Code, model.Description, model.DirectionId);
        await _service.UpdateAsync(id, request, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _service.DeleteAsync(id, ct);
    }
}
