using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Specification;
using Crm.Recruitment.Domain;
using Crm.Recruitment.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Crm.Recruitment.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IUserRepository))]
public class UserRepository : IUserRepository
{
    private readonly RecruitmentDbContext _context;

    public UserRepository(RecruitmentDbContext context) => _context = context;

    public async ValueTask<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Users.FindAsync([id], ct);

    public async ValueTask<User?> GetByIdNoTrackingAsync(Guid id, CancellationToken ct = default)
        => await _context.Users.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

    public async ValueTask<List<User>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var idList = ids.ToList();
        return await _context.Users
            .Where(u => idList.Contains(u.Id))
            .ToListAsync(ct);
    }

    public async ValueTask<List<User>> GetAllAsync(ISpecification<User>? spec = null, CancellationToken ct = default)
    {
        var query = _context.Users.AsQueryable();
        if (spec is not null)
            query = query.Where(spec.ToExpression());
        return await query.ToListAsync(ct);
    }

    public async ValueTask<List<User>> GetAllNoTrackingAsync(ISpecification<User>? spec = null, CancellationToken ct = default)
    {
        var query = _context.Users.AsNoTracking();
        if (spec is not null)
            query = query.Where(spec.ToExpression());
        return await query.ToListAsync(ct);
    }

    public async ValueTask<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        => await _context.Users.AnyAsync(u => u.Id == id, ct);

    public void Add(User entity) => _context.Users.Add(entity);

    public void Update(User entity) => _context.Users.Update(entity);

    public void Delete(User entity) => _context.Users.Remove(entity);

    public async ValueTask<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);
}
