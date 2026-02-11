using Cheetah.Core.EntityFramework.Configuration;
using Crm.Candidates.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Candidates.DataAccess.Configurations;

public class CandidateConfigurationOptions : AggregateRootConfigurationOptions<Candidate, Guid>
{
    public override string Schema => "candidates";
}

public class CandidateConfiguration : AggregateRootConfiguration<Candidate, Guid, CandidateConfigurationOptions>
{
    protected override CandidateConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<Candidate> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Description)
            .HasMaxLength(1024);

        builder.HasIndex(x => x.Name).IsUnique();
    }
}