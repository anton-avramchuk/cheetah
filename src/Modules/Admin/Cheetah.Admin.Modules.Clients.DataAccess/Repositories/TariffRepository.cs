using Cheetah.Admin.Modules.Clients.Domain;
using Cheetah.Admin.Modules.Clients.Domain.Repositories;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Specification;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Admin.Modules.Clients.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(ITariffRepository))]
public class TariffRepository : ITariffRepository
{
    private readonly ClientsDbContext _context;

    public TariffRepository(ClientsDbContext context) => _context = context;

    public async ValueTask<Tariff?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Tariffs.FindAsync([id], cancellationToken);

    public async ValueTask<Tariff?> GetByIdNoTrackingAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Tariffs.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async ValueTask<Tariff?> GetBySpecAsync(ISpecification<Tariff> spec, CancellationToken cancellationToken = default)
        => await _context.Tariffs.Where(spec.ToExpression()).FirstOrDefaultAsync(cancellationToken);

    public async ValueTask<List<Tariff>> GetAllAsync(ISpecification<Tariff>? spec = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Tariffs.AsQueryable();
        if (spec is not null)
            query = query.Where(spec.ToExpression());
        return await query.ToListAsync(cancellationToken);
    }

    public async ValueTask<List<Tariff>> GetAllNoTrackingAsync(ISpecification<Tariff>? spec = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Tariffs.AsNoTracking();
        if (spec is not null)
            query = query.Where(spec.ToExpression());
        return await query.ToListAsync(cancellationToken);
    }

    public async ValueTask<bool> ExistsAsync(ISpecification<Tariff> spec, CancellationToken cancellationToken = default)
        => await _context.Tariffs.AnyAsync(spec.ToExpression(), cancellationToken);

    public void Add(Tariff entity) => _context.Tariffs.Add(entity);

    public void Update(Tariff entity) => _context.Tariffs.Update(entity);

    public void Delete(Tariff entity) => _context.Tariffs.Remove(entity);

    public async ValueTask<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public IQueryable<Tariff> AsQueryable() => _context.Tariffs;

    public IQueryable<Tariff> AsNoTrackingQueryable() => _context.Tariffs.AsNoTracking();
}
