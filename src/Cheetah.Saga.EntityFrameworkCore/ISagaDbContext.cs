using Microsoft.EntityFrameworkCore;

namespace Cheetah.Saga.EntityFrameworkCore;

public interface ISagaDbContext
{
    DbSet<SagaInstance> SagaInstances { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
