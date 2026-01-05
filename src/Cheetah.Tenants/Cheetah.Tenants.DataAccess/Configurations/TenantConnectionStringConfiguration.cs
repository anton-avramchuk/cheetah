using Cheetah.Tenants.Domain.Entities;
using Cheetah.Tenants.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Tenants.DataAccess.Configurations;

public class TenantConnectionStringConfiguration : IEntityTypeConfiguration<TenantConnectionString>
{
    public void Configure(EntityTypeBuilder<TenantConnectionString> builder)
    {
        builder.ToTable("TenantConnectionStrings");

        builder.HasKey(cs => cs.Id);

        builder.Property(cs => cs.TenantId)
            .IsRequired();

        builder.Property(cs => cs.Name)
            .IsRequired()
            .HasMaxLength(TenantConstants.MaxConnectionStringNameLength);

        builder.Property(cs => cs.ConnectionString)
            .IsRequired()
            .HasMaxLength(TenantConstants.MaxConnectionStringLength);

        builder.Property(cs => cs.IsDefault)
            .IsRequired();

        builder.HasIndex(cs => new { cs.TenantId, cs.Name })
            .IsUnique();
    }
}
