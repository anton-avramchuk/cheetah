using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Core.EntityFramework;
using Cheetah.Permissions.Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Permissions.Catalog.DataAccess;

[ConnectionStringName("Permissions")]
public class PermissionsDbContext(DbContextOptions<PermissionsDbContext> options)
    : CrmDbContext<PermissionsDbContext>(options)
{
    public DbSet<PermissionDefinition> Permissions => Set<PermissionDefinition>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new PermissionDefinitionConfiguration());
    }
}

public class PermissionDefinitionConfiguration : IEntityTypeConfiguration<PermissionDefinition>
{
    public void Configure(EntityTypeBuilder<PermissionDefinition> builder)
    {
        builder.ToTable("Permissions", "permissions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(256);
        builder.Property(x => x.Description).HasMaxLength(1024).IsRequired();
        builder.Property(x => x.Module).HasMaxLength(256).IsRequired();
        builder.HasIndex(x => x.Module);
    }
}

public class PermissionsDbContextFactory : Microsoft.EntityFrameworkCore.Design.IDesignTimeDbContextFactory<PermissionsDbContext>
{
    public PermissionsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PermissionsDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=permissions;Username=postgres;Password=postgres");
        return new PermissionsDbContext(optionsBuilder.Options);
    }
}
