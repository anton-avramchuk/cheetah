using Cheetah.Core.Specification;

namespace Crm.Recruitment.Domain.Repositories;

public interface IVacancyRepository
{
    ValueTask<Vacancy?> GetByIdAsync(Guid id, CancellationToken ct = default);
    ValueTask<Vacancy?> GetByIdNoTrackingAsync(Guid id, CancellationToken ct = default);
    ValueTask<Vacancy?> GetBySpecAsync(ISpecification<Vacancy> spec, CancellationToken ct = default);
    ValueTask<List<Vacancy>> GetAllAsync(ISpecification<Vacancy>? spec = null, CancellationToken ct = default);

    ValueTask<List<Vacancy>>
        GetAllNoTrackingAsync(ISpecification<Vacancy>? spec = null, CancellationToken ct = default);

    ValueTask<bool> ExistsAsync(ISpecification<Vacancy> spec, CancellationToken ct = default);
    void Add(Vacancy entity);
    void Update(Vacancy entity);
    void Delete(Vacancy entity);
    ValueTask<int> SaveChangesAsync(CancellationToken ct = default);
    IQueryable<Vacancy> AsQueryable();
    IQueryable<Vacancy> AsNoTrackingQueryable();
}