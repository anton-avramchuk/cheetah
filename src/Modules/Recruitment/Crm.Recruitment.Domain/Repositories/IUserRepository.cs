using Cheetah.Core.Specification;

namespace Crm.Recruitment.Domain.Repositories;

public interface IUserRepository
{
    ValueTask<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    ValueTask<User?> GetByIdNoTrackingAsync(Guid id, CancellationToken ct = default);
    ValueTask<List<User>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    ValueTask<List<User>> GetAllAsync(ISpecification<User>? spec = null, CancellationToken ct = default);
    ValueTask<List<User>> GetAllNoTrackingAsync(ISpecification<User>? spec = null, CancellationToken ct = default);
    ValueTask<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    void Add(User entity);
    void Update(User entity);
    void Delete(User entity);
    ValueTask<int> SaveChangesAsync(CancellationToken ct = default);
}
