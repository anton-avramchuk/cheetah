using Cheetah.Audit;
using Cheetah.Audit.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Audit.Tests;

public class TestAuditDbContext : DbContext, IAuditDbContext
{
    public DbSet<TestCustomer> Customers => Set<TestCustomer>();
    public DbSet<TestNonAudited> Others => Set<TestNonAudited>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    public TestAuditDbContext(DbContextOptions<TestAuditDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.AddAudit();
        modelBuilder.Entity<TestCustomer>().HasKey(x => x.Id);
        modelBuilder.Entity<TestNonAudited>().HasKey(x => x.Id);
    }
}
