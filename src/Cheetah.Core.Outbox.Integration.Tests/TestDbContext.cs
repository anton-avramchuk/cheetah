using Cheetah.Core.Outbox;
using Cheetah.Core.Outbox.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Core.Outbox.Integration.Tests;

public class TestDbContext : DbContext, IOutboxDbContext, IDeadLetterDbContext
{
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;
    public DbSet<DeadLetterMessage> DeadLetterMessages { get; set; } = null!;

    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.AddOutbox();
        modelBuilder.AddDeadLetter();
    }
}
