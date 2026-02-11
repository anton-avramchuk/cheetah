using Crm.VacancyTasks.Contracts.Requests;
using Crm.VacancyTasks.Contracts.Response;

namespace Crm.VacancyTasks.ApiClient;

public interface IVacancyTasksService
{
    ValueTask<IReadOnlyList<VacancyTaskViewModel>> GetAllAsync(CancellationToken ct = default);
    ValueTask<VacancyTaskViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default);
    ValueTask<Guid> CreateAsync(CreateVacancyTaskRequest request, CancellationToken ct = default);
    ValueTask UpdateAsync(Guid id, UpdateVacancyTaskRequest request, CancellationToken ct = default);
    ValueTask DeleteAsync(Guid id, CancellationToken ct = default);
}