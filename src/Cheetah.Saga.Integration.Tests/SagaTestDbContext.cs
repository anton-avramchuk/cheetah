using Cheetah.Saga;
using Cheetah.Saga.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Saga.Integration.Tests;

public class SagaTestDbContext : DbContext, ISagaDbContext
{
    public DbSet<SagaInstance> SagaInstances => Set<SagaInstance>();

    public SagaTestDbContext(DbContextOptions<SagaTestDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.AddSagas();
    }
}
