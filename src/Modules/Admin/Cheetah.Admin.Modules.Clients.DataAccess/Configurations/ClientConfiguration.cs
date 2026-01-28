using Cheetah.Admin.Modules.Clients.Domain;
using Cheetah.Core.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Admin.Modules.Clients.DataAccess.Configurations;

public class ClientConfigurationOptions : AggregateRootConfigurationOptions<Client, Guid>
{
    public override string Schema => "clients";
}

public class ClientConfiguration : AggregateRootConfiguration<Client, Guid, ClientConfigurationOptions>
{
    protected override ClientConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<Client> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Description)
            .HasMaxLength(1024);

        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasIndex(x => x.TenantId);
    }
}