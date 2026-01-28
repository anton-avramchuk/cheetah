using Cheetah.Core.Specification;

namespace Cheetah.Admin.Modules.Clients.Domain.Repositories;

public interface IClientRepository
{
    ValueTask<Client?> GetByIdAsync(Guid id, CancellationToken ct = default);
    ValueTask<Client?> GetBySpecAsync(ISpecification<Client> spec, CancellationToken ct = default);
    ValueTask<List<Client>> GetAllAsync(ISpecification<Client>? spec = null, CancellationToken ct = default);
    ValueTask<bool> ExistsAsync(ISpecification<Client> spec, CancellationToken ct = default);
    void Add(Client entity);
    void Update(Client entity);
    void Delete(Client entity);
    ValueTask<int> SaveChangesAsync(CancellationToken ct = default);
    IQueryable<Client> AsQueryable();
    IQueryable<Client> AsNoTrackingQueryable();
}
