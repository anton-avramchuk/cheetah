using Cheetah.Core.Specification;

namespace Crm.Recruitment.Domain.Repositories;

public interface IVacancyAssignmentRepository
{
    ValueTask<VacancyAssignment?> GetByIdAsync(Guid id, CancellationToken ct = default);

    ValueTask<List<VacancyAssignment>> GetByVacancyIdAsync(
        Guid vacancyId,
        CancellationToken ct = default);

    ValueTask<List<VacancyAssignment>> GetByVacancyIdWithDetailsAsync(
        Guid vacancyId,
        CancellationToken ct = default);

    ValueTask<VacancyAssignment?> GetByVacancyAndRoleAsync(
        Guid vacancyId,
        Guid roleId,
        CancellationToken ct = default);

    ValueTask<List<VacancyAssignment>> GetByUserIdAsync(
        Guid userId,
        CancellationToken ct = default);

    ValueTask<bool> ExistsAsync(
        Guid vacancyId,
        Guid userId,
        Guid roleId,
        CancellationToken ct = default);

    ValueTask<int> CountByVacancyAndRoleAsync(
        Guid vacancyId,
        Guid roleId,
        CancellationToken ct = default);

    void Add(VacancyAssignment entity);
    void Delete(VacancyAssignment entity);
    ValueTask<int> SaveChangesAsync(CancellationToken ct = default);
}
