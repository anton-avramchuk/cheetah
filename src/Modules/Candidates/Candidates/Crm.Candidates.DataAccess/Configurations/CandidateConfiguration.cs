using Cheetah.Core.EntityFramework.Configuration;
using Crm.Candidates.Domain;
using Microsoft.EntityFrameworkCore;
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

        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Email)
            .HasMaxLength(256);

        builder.Property(x => x.Phone)
            .HasMaxLength(50);

        builder.Property(x => x.City)
            .HasMaxLength(256);

        builder.Property(x => x.CurrentPosition)
            .HasMaxLength(256);

        builder.Property(x => x.CurrentCompany)
            .HasMaxLength(256);

        builder.Property(x => x.About)
            .HasMaxLength(4000);

        builder.Navigation(x => x.ExternalProfiles)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(x => x.Comments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
