using Crm.Recruitment.Contracts.Requests;
using Crm.Recruitment.Contracts.Response;

namespace Crm.Recruitment.ApiClient;

public interface IRecruitmentService
{
    ValueTask<IReadOnlyList<VacancyViewModel>> GetAllAsync(CancellationToken ct = default);
    ValueTask<VacancyViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default);
    ValueTask<Guid> CreateAsync(CreateVacancyRequest request, CancellationToken ct = default);
    ValueTask UpdateAsync(Guid id, UpdateVacancyRequest request, CancellationToken ct = default);
    ValueTask DeleteAsync(Guid id, CancellationToken ct = default);
}