using Cheetah.Core.Specification;

namespace __Prefix__.ModuleName.Domain.Repositories;

public interface ISampleEntityRepository
{
    ValueTask<SampleEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
    ValueTask<SampleEntity?> GetByIdNoTrackingAsync(Guid id, CancellationToken ct = default);
    ValueTask<SampleEntity?> GetBySpecAsync(ISpecification<SampleEntity> spec, CancellationToken ct = default);
    ValueTask<List<SampleEntity>> GetAllAsync(ISpecification<SampleEntity>? spec = null, CancellationToken ct = default);
    ValueTask<List<SampleEntity>> GetAllNoTrackingAsync(ISpecification<SampleEntity>? spec = null, CancellationToken ct = default);
    ValueTask<bool> ExistsAsync(ISpecification<SampleEntity> spec, CancellationToken ct = default);
    void Add(SampleEntity entity);
    void Update(SampleEntity entity);
    void Delete(SampleEntity entity);
    ValueTask<int> SaveChangesAsync(CancellationToken ct = default);
    IQueryable<SampleEntity> AsQueryable();
    IQueryable<SampleEntity> AsNoTrackingQueryable();
}
