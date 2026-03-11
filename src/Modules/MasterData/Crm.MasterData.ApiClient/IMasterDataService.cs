using Crm.MasterData.Contracts.Requests;
using Crm.MasterData.Contracts.Response;

namespace Crm.MasterData.ApiClient;

public interface IMasterDataService
{
    ValueTask<IReadOnlyList<StackItemViewModel>> GetAllAsync(CancellationToken ct = default);
    ValueTask<StackItemViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default);
    ValueTask<Guid> CreateAsync(CreateStackItemRequest request, CancellationToken ct = default);
    ValueTask UpdateAsync(Guid id, UpdateStackItemRequest request, CancellationToken ct = default);
    ValueTask DeleteAsync(Guid id, CancellationToken ct = default);
}