using Crm.Customer.Contracts.Requests;
using Crm.Customer.Contracts.Response;

namespace Crm.Customer.ApiClient;

public interface ICustomerService
{
    ValueTask<IReadOnlyList<CustomerViewModel>> GetAllAsync(CancellationToken ct = default);
    ValueTask<CustomerViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default);
    ValueTask<Guid> CreateAsync(CreateCustomerRequest request, CancellationToken ct = default);
    ValueTask UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken ct = default);
    ValueTask DeleteAsync(Guid id, CancellationToken ct = default);
}