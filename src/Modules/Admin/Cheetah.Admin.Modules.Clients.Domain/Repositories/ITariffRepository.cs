using Cheetah.Core.Specification;

namespace Cheetah.Admin.Modules.Clients.Domain.Repositories;

public interface ITariffRepository
{
    ValueTask<Tariff?> GetByIdAsync(Guid id, CancellationToken ct = default);
    ValueTask<Tariff?> GetByIdNoTrackingAsync(Guid id, CancellationToken ct = default);
    ValueTask<Tariff?> GetBySpecAsync(ISpecification<Tariff> spec, CancellationToken ct = default);
    ValueTask<List<Tariff>> GetAllAsync(ISpecification<Tariff>? spec = null, CancellationToken ct = default);
    ValueTask<List<Tariff>> GetAllNoTrackingAsync(ISpecification<Tariff>? spec = null, CancellationToken ct = default);
    ValueTask<bool> ExistsAsync(ISpecification<Tariff> spec, CancellationToken ct = default);
    void Add(Tariff entity);
    void Update(Tariff entity);
    void Delete(Tariff entity);
    ValueTask<int> SaveChangesAsync(CancellationToken ct = default);
    IQueryable<Tariff> AsQueryable();
    IQueryable<Tariff> AsNoTrackingQueryable();
}
