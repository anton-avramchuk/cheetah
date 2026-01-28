using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Cheetah.Core.EntityFramework;

[Export(LifetimeType.Scoped)]
public abstract class CrmDbContext<TDbContext> : DbContext, ICrmDbContext
    where TDbContext : DbContext
{
    protected CrmDbContext(DbContextOptions<TDbContext> options)
        : base(options)
    {
    }

    public IDbContextTransaction CreateTransaction()
    {
        return Database.BeginTransaction();
    }


    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        if (ChangeTracker.HasChanges())
        {
            var date = DateTimeOffset.UtcNow;
            foreach (var entry in ChangeTracker.Entries()
                         .Where(w => w.State is EntityState.Added or EntityState.Modified))
            {
                if (entry.Entity is IUpdatedAtEntity updatedEntity)
                {
                    updatedEntity.UpdatedAt = date;
                }

                if (entry is { Entity: ICreateAtEntity createdEntity, State: EntityState.Added })
                {
                    createdEntity.CreatedAt = date;
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}