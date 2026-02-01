using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Specification;
using __Prefix__.ModuleName.Domain;
using __Prefix__.ModuleName.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace __Prefix__.ModuleName.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(ISampleEntityRepository))]
public class SampleEntityRepository : ISampleEntityRepository
{
    private readonly ModuleNameDbContext _context;

    public SampleEntityRepository(ModuleNameDbContext context) => _context = context;

    public async ValueTask<SampleEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.SampleEntities.FindAsync([id], ct);

    public async ValueTask<SampleEntity?> GetByIdNoTrackingAsync(Guid id, CancellationToken ct = default)
        => await _context.SampleEntities.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

    public async ValueTask<SampleEntity?> GetBySpecAsync(ISpecification<SampleEntity> spec, CancellationToken ct = default)
        => await _context.SampleEntities.Where(spec.ToExpression()).FirstOrDefaultAsync(ct);

    public async ValueTask<List<SampleEntity>> GetAllAsync(ISpecification<SampleEntity>? spec = null, CancellationToken ct = default)
    {
        var query = _context.SampleEntities.AsQueryable();
        if (spec is not null)
            query = query.Where(spec.ToExpression());
        return await query.ToListAsync(ct);
    }

    public async ValueTask<List<SampleEntity>> GetAllNoTrackingAsync(ISpecification<SampleEntity>? spec = null, CancellationToken ct = default)
    {
        var query = _context.SampleEntities.AsNoTracking();
        if (spec is not null)
            query = query.Where(spec.ToExpression());
        return await query.ToListAsync(ct);
    }

    public async ValueTask<bool> ExistsAsync(ISpecification<SampleEntity> spec, CancellationToken ct = default)
        => await _context.SampleEntities.AnyAsync(spec.ToExpression(), ct);

    public void Add(SampleEntity entity) => _context.SampleEntities.Add(entity);

    public void Update(SampleEntity entity) => _context.SampleEntities.Update(entity);

    public void Delete(SampleEntity entity) => _context.SampleEntities.Remove(entity);

    public async ValueTask<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);

    public IQueryable<SampleEntity> AsQueryable() => _context.SampleEntities;

    public IQueryable<SampleEntity> AsNoTrackingQueryable() => _context.SampleEntities.AsNoTracking();
}
