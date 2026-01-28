using Cheetah.Admin.Modules.Clients.Domain;
using Cheetah.Admin.Modules.Clients.Domain.Repositories;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Specification;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Admin.Modules.Clients.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IClientRepository))]
public class ClientRepository : IClientRepository
{
    private readonly ClientsDbContext _context;

    public ClientRepository(ClientsDbContext context) => _context = context;

    public async ValueTask<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Clients.FindAsync([id], cancellationToken);

    public async ValueTask<Client?> GetBySpecAsync(ISpecification<Client> spec, CancellationToken cancellationToken = default)
        => await _context.Clients.Where(spec.ToExpression()).FirstOrDefaultAsync(cancellationToken);

    public async ValueTask<List<Client>> GetAllAsync(ISpecification<Client>? spec = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Clients.AsQueryable();
        if (spec is not null)
            query = query.Where(spec.ToExpression());
        return await query.ToListAsync(cancellationToken);
    }

    public async ValueTask<bool> ExistsAsync(ISpecification<Client> spec, CancellationToken cancellationToken = default)
        => await _context.Clients.AnyAsync(spec.ToExpression(), cancellationToken);

    public void Add(Client entity) => _context.Clients.Add(entity);

    public void Update(Client entity) => _context.Clients.Update(entity);

    public void Delete(Client entity) => _context.Clients.Remove(entity);

    public async ValueTask<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public IQueryable<Client> AsQueryable() => _context.Clients;

    public IQueryable<Client> AsNoTrackingQueryable() => _context.Clients.AsNoTracking();
}
