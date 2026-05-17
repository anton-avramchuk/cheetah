using Microsoft.EntityFrameworkCore;

namespace Cheetah.Saga.EntityFrameworkCore;

public sealed class EfSagaRepository<TContext> : ISagaRepository
    where TContext : DbContext, ISagaDbContext
{
    private readonly TContext _context;

    public EfSagaRepository(TContext context) => _context = context;

    public async ValueTask<SagaInstance?> FindAsync(string sagaType, string correlationKey, CancellationToken cancellationToken = default)
    {
        return await _context.SagaInstances
            .FirstOrDefaultAsync(x => x.SagaType == sagaType && x.CorrelationKey == correlationKey, cancellationToken)
            .ConfigureAwait(false);
    }

    public ValueTask AddAsync(SagaInstance instance, CancellationToken cancellationToken = default)
    {
        _context.SagaInstances.Add(instance);
        return ValueTask.CompletedTask;
    }

    public async ValueTask SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Перед save увеличиваем версию у модифицированных instance — для optimistic locking.
            foreach (var entry in _context.ChangeTracker.Entries<SagaInstance>())
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.Version += 1;
                }
            }

            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var conflict = ex.Entries.OfType<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<SagaInstance>>()
                .FirstOrDefault()?.Entity;
            throw new SagaConcurrencyException(
                conflict?.SagaType ?? "<unknown>",
                conflict?.CorrelationKey ?? "<unknown>");
        }
    }
}
