using Cheetah.Core.DataAccess;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Tenants.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Tenants.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IRepository<Tenant, Guid>), typeof(IReadOnlyRepository<Tenant, Guid>),
    typeof(ITenantRepository))]
public class TenantRepository : ITenantRepository
{
    private readonly TenantsDbContext _context;

    public TenantRepository(TenantsDbContext context)
    {
        _context = context;
    }

    public async Task<Tenant?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .Include(t => t.ConnectionStrings)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Tenant?> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<Tenant, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .Include(t => t.ConnectionStrings)
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .Include(t => t.ConnectionStrings)
            .ToListAsync(cancellationToken);
    }

    public async Task<Tenant?> GetByIdAsync(Guid key, CancellationToken cancellationToken = default)
    {
        return await _context.Tenants
            .Include(t => t.ConnectionStrings)
            .FirstOrDefaultAsync(x => x.Id == key, cancellationToken);
    }

    public async Task InsertAsync(Tenant entity, CancellationToken cancellationToken = default)
    {
        await _context.Tenants.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Tenant entity, CancellationToken cancellationToken = default)
    {
        _context.Tenants.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Tenant entity, CancellationToken cancellationToken = default)
    {
        _context.Tenants.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}