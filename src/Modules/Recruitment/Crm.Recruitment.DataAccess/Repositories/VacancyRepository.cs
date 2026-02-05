using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Specification;
using Crm.Recruitment.Domain;
using Crm.Recruitment.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Crm.Recruitment.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IVacancyRepository))]
public class VacancyRepository : IVacancyRepository
{
    private readonly RecruitmentDbContext _context;

    public VacancyRepository(RecruitmentDbContext context) => _context = context;

    public async ValueTask<Vacancy?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Vacancies.FindAsync([id], ct);

    public async ValueTask<Vacancy?> GetByIdNoTrackingAsync(Guid id, CancellationToken ct = default)
        => await _context.Vacancies.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

    public async ValueTask<Vacancy?> GetBySpecAsync(ISpecification<Vacancy> spec, CancellationToken ct = default)
        => await _context.Vacancies.Where(spec.ToExpression()).FirstOrDefaultAsync(ct);

    public async ValueTask<List<Vacancy>> GetAllAsync(ISpecification<Vacancy>? spec = null,
        CancellationToken ct = default)
    {
        var query = _context.Vacancies.AsQueryable();
        if (spec is not null)
            query = query.Where(spec.ToExpression());
        return await query.ToListAsync(ct);
    }

    public async ValueTask<List<Vacancy>> GetAllNoTrackingAsync(ISpecification<Vacancy>? spec = null,
        CancellationToken ct = default)
    {
        var query = _context.Vacancies.AsNoTracking();
        if (spec is not null)
            query = query.Where(spec.ToExpression());
        return await query.ToListAsync(ct);
    }

    public async ValueTask<bool> ExistsAsync(ISpecification<Vacancy> spec, CancellationToken ct = default)
        => await _context.Vacancies.AnyAsync(spec.ToExpression(), ct);

    public void Add(Vacancy entity) => _context.Vacancies.Add(entity);

    public void Update(Vacancy entity) => _context.Vacancies.Update(entity);

    public void Delete(Vacancy entity) => _context.Vacancies.Remove(entity);

    public async ValueTask<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);

    public IQueryable<Vacancy> AsQueryable() => _context.Vacancies;

    public IQueryable<Vacancy> AsNoTrackingQueryable() => _context.Vacancies.AsNoTracking();
}