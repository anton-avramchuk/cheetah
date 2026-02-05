using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Specification;
using Crm.Recruitment.Domain;
using Crm.Recruitment.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Crm.Recruitment.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IVacancyStateRepository))]
public class VacancyStateRepository : IVacancyStateRepository
{
    private readonly RecruitmentDbContext _context;

    public VacancyStateRepository(RecruitmentDbContext context) => _context = context;

    public async ValueTask<VacancyState?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.VacancyStates.FindAsync([id], ct);

    public async ValueTask<VacancyState?> GetByIdNoTrackingAsync(Guid id, CancellationToken ct = default)
        => await _context.VacancyStates.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

    public async ValueTask<VacancyState?> GetBySpecAsync(ISpecification<VacancyState> spec, CancellationToken ct = default)
        => await _context.VacancyStates.Where(spec.ToExpression()).FirstOrDefaultAsync(ct);

    public async ValueTask<List<VacancyState>> GetAllAsync(
        ISpecification<VacancyState>? spec = null,
        CancellationToken ct = default)
    {
        var query = _context.VacancyStates.AsQueryable();
        if (spec is not null)
            query = query.Where(spec.ToExpression());
        return await query.ToListAsync(ct);
    }

    public async ValueTask<List<VacancyState>> GetAllNoTrackingAsync(
        ISpecification<VacancyState>? spec = null,
        CancellationToken ct = default)
    {
        var query = _context.VacancyStates.AsNoTracking();
        if (spec is not null)
            query = query.Where(spec.ToExpression());
        return await query.ToListAsync(ct);
    }

    public async ValueTask<bool> ExistsAsync(ISpecification<VacancyState> spec, CancellationToken ct = default)
        => await _context.VacancyStates.AnyAsync(spec.ToExpression(), ct);

    public void Add(VacancyState entity) => _context.VacancyStates.Add(entity);

    public void Update(VacancyState entity) => _context.VacancyStates.Update(entity);

    public void Delete(VacancyState entity) => _context.VacancyStates.Remove(entity);

    public async ValueTask<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);

    public IQueryable<VacancyState> AsQueryable() => _context.VacancyStates;

    public IQueryable<VacancyState> AsNoTrackingQueryable() => _context.VacancyStates.AsNoTracking();
}
