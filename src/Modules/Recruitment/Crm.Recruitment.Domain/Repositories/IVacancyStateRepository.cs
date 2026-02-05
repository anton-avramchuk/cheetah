using Cheetah.Core.Specification;

namespace Crm.Recruitment.Domain.Repositories;

public interface IVacancyStateRepository
{
    ValueTask<VacancyState?> GetByIdAsync(Guid id, CancellationToken ct = default);
    ValueTask<VacancyState?> GetByIdNoTrackingAsync(Guid id, CancellationToken ct = default);
    ValueTask<VacancyState?> GetBySpecAsync(ISpecification<VacancyState> spec, CancellationToken ct = default);
    ValueTask<List<VacancyState>> GetAllAsync(ISpecification<VacancyState>? spec = null, CancellationToken ct = default);

    ValueTask<List<VacancyState>> GetAllNoTrackingAsync(
        ISpecification<VacancyState>? spec = null,
        CancellationToken ct = default);

    ValueTask<bool> ExistsAsync(ISpecification<VacancyState> spec, CancellationToken ct = default);
    void Add(VacancyState entity);
    void Update(VacancyState entity);
    void Delete(VacancyState entity);
    ValueTask<int> SaveChangesAsync(CancellationToken ct = default);
    IQueryable<VacancyState> AsQueryable();
    IQueryable<VacancyState> AsNoTrackingQueryable();
}
