using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Modularity;
using Cheetah.Core.Specification;
using Cheetah.Identity.Domain.Entities;
using Cheetah.Identity.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Identity.DataAccess.Repositories;

/// <summary>
/// Repository implementation for Role entity
/// </summary>
[Export(LifetimeType.Scoped, typeof(IRoleRepository))]
public class RoleRepository : IRoleRepository
{
    private readonly IdentityDbContext _context;

    public RoleRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Role?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Roles.FindAsync([id], ct);
    }

    public async ValueTask<Role?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        var normalizedName = name.ToUpperInvariant();
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.NormalizedName == normalizedName, ct);
    }

    public async ValueTask<Role?> GetBySpecAsync(ISpecification<Role> spec, CancellationToken ct = default)
    {
        return await _context.Roles
            .Where(spec.ToExpression())
            .FirstOrDefaultAsync(ct);
    }

    public async ValueTask<Role?> GetBySpecWithIncludesAsync(
        ISpecification<Role> spec,
        bool includeClaims = false,
        CancellationToken ct = default)
    {
        var query = _context.Roles.AsQueryable();

        if (includeClaims)
            query = query.Include(r => r.Claims);

        return await query
            .Where(spec.ToExpression())
            .FirstOrDefaultAsync(ct);
    }

    public async ValueTask<List<Role>> GetAllAsync(ISpecification<Role>? spec = null, CancellationToken ct = default)
    {
        var query = _context.Roles.AsQueryable();

        if (spec != null)
            query = query.Where(spec.ToExpression());

        return await query.ToListAsync(ct);
    }

    public async ValueTask<List<Role>> GetAllWithIncludesAsync(
        ISpecification<Role>? spec = null,
        bool includeClaims = false,
        CancellationToken ct = default)
    {
        var query = _context.Roles.AsQueryable();

        if (includeClaims)
            query = query.Include(r => r.Claims);

        if (spec != null)
            query = query.Where(spec.ToExpression());

        return await query.ToListAsync(ct);
    }

    public async ValueTask<bool> ExistsAsync(ISpecification<Role> spec, CancellationToken ct = default)
    {
        return await _context.Roles
            .AnyAsync(spec.ToExpression(), ct);
    }

    public void Add(Role role)
    {
        _context.Roles.Add(role);
    }

    public void Update(Role role)
    {
        _context.Roles.Update(role);
    }

    public void Delete(Role role)
    {
        _context.Roles.Remove(role);
    }

    public async ValueTask<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }

    public IQueryable<Role> AsQueryable()
    {
        return _context.Roles.AsQueryable();
    }

    public IQueryable<Role> AsNoTrackingQueryable()
    {
        return _context.Roles.AsNoTracking();
    }
}
