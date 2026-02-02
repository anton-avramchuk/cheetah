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
        => await _context.SampleEntities.FindAsync([id], ct);

    public async ValueTask<Vacancy?> GetByIdNoTrackingAsync(Guid id, CancellationToken ct = default)
        => await _context.SampleEntities.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

    public async ValueTask<Vacancy?> GetBySpecAsync(ISpecification<Vacancy> spec, CancellationToken ct = default)
        => await _context.SampleEntities.Where(spec.ToExpression()).FirstOrDefaultAsync(ct);

    public async ValueTask<List<Vacancy>> GetAllAsync(ISpecification<Vacancy>? spec = null,
        CancellationToken ct = default)
    {
        var query = _context.SampleEntities.AsQueryable();
        if (spec is not null)
            query = query.Where(spec.ToExpression());
        return await query.ToListAsync(ct);
    }

    public async ValueTask<List<Vacancy>> GetAllNoTrackingAsync(ISpecification<Vacancy>? spec = null,
        CancellationToken ct = default)
    {
        var query = _context.SampleEntities.AsNoTracking();
        if (spec is not null)
            query = query.Where(spec.ToExpression());
        return await query.ToListAsync(ct);
    }

    public async ValueTask<bool> ExistsAsync(ISpecification<Vacancy> spec, CancellationToken ct = default)
        => await _context.SampleEntities.AnyAsync(spec.ToExpression(), ct);

    public void Add(Vacancy entity) => _context.SampleEntities.Add(entity);

    public void Update(Vacancy entity) => _context.SampleEntities.Update(entity);

    public void Delete(Vacancy entity) => _context.SampleEntities.Remove(entity);

    public async ValueTask<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);

    public IQueryable<Vacancy> AsQueryable() => _context.SampleEntities;

    public IQueryable<Vacancy> AsNoTrackingQueryable() => _context.SampleEntities.AsNoTracking();
}