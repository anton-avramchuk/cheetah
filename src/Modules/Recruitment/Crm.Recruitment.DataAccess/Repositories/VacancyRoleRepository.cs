using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Specification;
using Crm.Recruitment.Domain;
using Crm.Recruitment.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Crm.Recruitment.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IVacancyRoleRepository))]
public class VacancyRoleRepository : IVacancyRoleRepository
{
    private readonly RecruitmentDbContext _context;

    public VacancyRoleRepository(RecruitmentDbContext context) => _context = context;

    public async ValueTask<VacancyRole?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.VacancyRoles.FindAsync([id], ct);

    public async ValueTask<VacancyRole?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await _context.VacancyRoles.FirstOrDefaultAsync(r => r.Code == code.ToLowerInvariant(), ct);

    public async ValueTask<List<VacancyRole>> GetAllAsync(ISpecification<VacancyRole>? spec = null, CancellationToken ct = default)
    {
        var query = _context.VacancyRoles.AsQueryable();
        if (spec is not null)
            query = query.Where(spec.ToExpression());
        return await query.OrderBy(r => r.Order).ToListAsync(ct);
    }

    public async ValueTask<List<VacancyRole>> GetAllNoTrackingAsync(ISpecification<VacancyRole>? spec = null, CancellationToken ct = default)
    {
        var query = _context.VacancyRoles.AsNoTracking();
        if (spec is not null)
            query = query.Where(spec.ToExpression());
        return await query.OrderBy(r => r.Order).ToListAsync(ct);
    }

    public async ValueTask<bool> ExistsAsync(ISpecification<VacancyRole> spec, CancellationToken ct = default)
        => await _context.VacancyRoles.AnyAsync(spec.ToExpression(), ct);

    public void Add(VacancyRole entity) => _context.VacancyRoles.Add(entity);

    public void Update(VacancyRole entity) => _context.VacancyRoles.Update(entity);

    public void Delete(VacancyRole entity) => _context.VacancyRoles.Remove(entity);

    public async ValueTask<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);
}
