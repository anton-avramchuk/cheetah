using Crm.Candidates.Contracts.Requests;
using Crm.Candidates.Contracts.Response;

namespace Crm.Candidates.ApiClient;

public interface ICandidatesService
{
    ValueTask<IReadOnlyList<CandidateViewModel>> GetAllAsync(CancellationToken ct = default);
    ValueTask<CandidateViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default);
    ValueTask<Guid> CreateAsync(CreateCandidateRequest request, CancellationToken ct = default);
    ValueTask UpdateAsync(Guid id, UpdateCandidateRequest request, CancellationToken ct = default);
    ValueTask DeleteAsync(Guid id, CancellationToken ct = default);
}