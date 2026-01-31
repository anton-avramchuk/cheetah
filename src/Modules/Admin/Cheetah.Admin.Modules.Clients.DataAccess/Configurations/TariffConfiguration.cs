using Cheetah.Admin.Modules.Clients.Domain;
using Cheetah.Core.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Admin.Modules.Clients.DataAccess.Configurations;

public class TariffConfigurationOptions : AggregateRootConfigurationOptions<Tariff, Guid>
{
    public override string Schema => "clients";
}

public class TariffConfiguration : AggregateRootConfiguration<Tariff, Guid, TariffConfigurationOptions>
{
    protected override TariffConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<Tariff> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Description)
            .HasMaxLength(1024);

        builder.Property(x => x.Price)
            .HasPrecision(18, 2);

        builder.Property(x => x.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.HasIndex(x => x.Name).IsUnique();
    }
}
