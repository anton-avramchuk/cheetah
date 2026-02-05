using Cheetah.Core.Specification;

namespace Crm.Recruitment.Domain.Repositories;

public interface IVacancyRoleRepository
{
    ValueTask<VacancyRole?> GetByIdAsync(Guid id, CancellationToken ct = default);
    ValueTask<VacancyRole?> GetByCodeAsync(string code, CancellationToken ct = default);
    ValueTask<List<VacancyRole>> GetAllAsync(ISpecification<VacancyRole>? spec = null, CancellationToken ct = default);
    ValueTask<List<VacancyRole>> GetAllNoTrackingAsync(ISpecification<VacancyRole>? spec = null, CancellationToken ct = default);
    ValueTask<bool> ExistsAsync(ISpecification<VacancyRole> spec, CancellationToken ct = default);
    void Add(VacancyRole entity);
    void Update(VacancyRole entity);
    void Delete(VacancyRole entity);
    ValueTask<int> SaveChangesAsync(CancellationToken ct = default);
}
