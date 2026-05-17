using Cheetah.Audit;
using Cheetah.Audit.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Audit.Integration.Tests;

public class AuditDbContext : DbContext, IAuditDbContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Customer>().HasKey(x => x.Id);
        modelBuilder.AddAudit();
    }
}
