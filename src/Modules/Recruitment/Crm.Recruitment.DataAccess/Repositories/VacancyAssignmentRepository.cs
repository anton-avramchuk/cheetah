using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.Domain;
using Crm.Recruitment.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Crm.Recruitment.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IVacancyAssignmentRepository))]
public class VacancyAssignmentRepository : IVacancyAssignmentRepository
{
    private readonly RecruitmentDbContext _context;

    public VacancyAssignmentRepository(RecruitmentDbContext context) => _context = context;

    public async ValueTask<VacancyAssignment?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.VacancyAssignments.FindAsync([id], ct);

    public async ValueTask<List<VacancyAssignment>> GetByVacancyIdAsync(Guid vacancyId, CancellationToken ct = default)
        => await _context.VacancyAssignments
            .Where(a => a.VacancyId == vacancyId)
            .ToListAsync(ct);

    public async ValueTask<List<VacancyAssignment>> GetByVacancyIdWithDetailsAsync(Guid vacancyId, CancellationToken ct = default)
        => await _context.VacancyAssignments
            .Include(a => a.User)
            .Include(a => a.Role)
            .Where(a => a.VacancyId == vacancyId)
            .OrderBy(a => a.Role!.Order)
            .ToListAsync(ct);

    public async ValueTask<VacancyAssignment?> GetByVacancyAndRoleAsync(Guid vacancyId, Guid roleId, CancellationToken ct = default)
        => await _context.VacancyAssignments
            .FirstOrDefaultAsync(a => a.VacancyId == vacancyId && a.RoleId == roleId, ct);

    public async ValueTask<List<VacancyAssignment>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _context.VacancyAssignments
            .Include(a => a.Vacancy)
            .Include(a => a.Role)
            .Where(a => a.UserId == userId)
            .ToListAsync(ct);

    public async ValueTask<bool> ExistsAsync(Guid vacancyId, Guid userId, Guid roleId, CancellationToken ct = default)
        => await _context.VacancyAssignments
            .AnyAsync(a => a.VacancyId == vacancyId && a.UserId == userId && a.RoleId == roleId, ct);

    public async ValueTask<int> CountByVacancyAndRoleAsync(Guid vacancyId, Guid roleId, CancellationToken ct = default)
        => await _context.VacancyAssignments
            .CountAsync(a => a.VacancyId == vacancyId && a.RoleId == roleId, ct);

    public void Add(VacancyAssignment entity) => _context.VacancyAssignments.Add(entity);

    public void Delete(VacancyAssignment entity) => _context.VacancyAssignments.Remove(entity);

    public async ValueTask<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);
}
