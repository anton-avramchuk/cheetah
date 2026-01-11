using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Modularity;
using Cheetah.Core.Specification;
using Cheetah.Identity.Domain.Entities;
using Cheetah.Identity.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Identity.DataAccess.Repositories;

/// <summary>
/// Repository implementation for User entity
/// </summary>
[Export(LifetimeType.Scoped, typeof(IUserRepository))]
public class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _context;

    public UserRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async ValueTask<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Users.FindAsync([id], ct);
    }

    public async ValueTask<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalizedEmail = email.ToUpperInvariant();
        return await _context.Users
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, ct);
    }

    public async ValueTask<User?> GetBySpecAsync(ISpecification<User> spec, CancellationToken ct = default)
    {
        return await _context.Users
            .Where(spec.ToExpression())
            .FirstOrDefaultAsync(ct);
    }

    public async ValueTask<User?> GetBySpecWithIncludesAsync(
        ISpecification<User> spec,
        bool includeRoles = false,
        bool includeClaims = false,
        CancellationToken ct = default)
    {
        var query = _context.Users.AsQueryable();

        if (includeRoles)
            query = query.Include(u => u.Roles);

        if (includeClaims)
            query = query.Include(u => u.Claims);

        return await query
            .Where(spec.ToExpression())
            .FirstOrDefaultAsync(ct);
    }

    public async ValueTask<List<User>> GetAllAsync(ISpecification<User>? spec = null, CancellationToken ct = default)
    {
        var query = _context.Users.AsQueryable();

        if (spec != null)
            query = query.Where(spec.ToExpression());

        return await query.ToListAsync(ct);
    }

    public async ValueTask<List<User>> GetAllWithIncludesAsync(
        ISpecification<User>? spec = null,
        bool includeRoles = false,
        bool includeClaims = false,
        CancellationToken ct = default)
    {
        var query = _context.Users.AsQueryable();

        if (includeRoles)
            query = query.Include(u => u.Roles);

        if (includeClaims)
            query = query.Include(u => u.Claims);

        if (spec != null)
            query = query.Where(spec.ToExpression());

        return await query.ToListAsync(ct);
    }

    public async ValueTask<bool> ExistsAsync(ISpecification<User> spec, CancellationToken ct = default)
    {
        return await _context.Users
            .AnyAsync(spec.ToExpression(), ct);
    }

    public void Add(User user)
    {
        _context.Users.Add(user);
    }

    public void Update(User user)
    {
        _context.Users.Update(user);
    }

    public void Delete(User user)
    {
        _context.Users.Remove(user);
    }

    public async ValueTask<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }

    public IQueryable<User> AsQueryable()
    {
        return _context.Users.AsQueryable();
    }

    public IQueryable<User> AsNoTrackingQueryable()
    {
        return _context.Users.AsNoTracking();
    }
}
