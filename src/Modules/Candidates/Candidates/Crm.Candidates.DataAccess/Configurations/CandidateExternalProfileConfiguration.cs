using Cheetah.Core.EntityFramework.Configuration;
using Crm.Candidates.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Candidates.DataAccess.Configurations;

public class CandidateExternalProfileConfigurationOptions : EntityConfigurationOptions<CandidateExternalProfile, Guid>
{
    public override string Schema => "candidates";
}

public class CandidateExternalProfileConfiguration : EntityConfiguration<CandidateExternalProfile, Guid, CandidateExternalProfileConfigurationOptions>
{
    protected override CandidateExternalProfileConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<CandidateExternalProfile> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Url).HasMaxLength(2048);
        builder.Property(x => x.ExternalId).HasMaxLength(256);

        builder.HasOne(x => x.Source)
            .WithMany()
            .HasForeignKey(x => x.SourceId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Candidate>()
            .WithMany(x => x.ExternalProfiles)
            .HasForeignKey(x => x.CandidateId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.SourceId, x.ExternalId })
            .IsUnique()
            .HasFilter("\"ExternalId\" IS NOT NULL");
    }
}
