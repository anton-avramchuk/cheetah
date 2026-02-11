using Cheetah.Core.EntityFramework.Configuration;
using Crm.Candidates.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Candidates.DataAccess.Configurations;

public class CandidateApplicationConfigurationOptions : AggregateRootConfigurationOptions<CandidateApplication, Guid>
{
    public override string Schema => "candidates";
}

public class CandidateApplicationConfiguration : AggregateRootConfiguration<CandidateApplication, Guid, CandidateApplicationConfigurationOptions>
{
    protected override CandidateApplicationConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<CandidateApplication> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.VacancyId).IsRequired();
        builder.HasIndex(x => x.VacancyId);

        builder.Property(x => x.Order).IsRequired();

        builder.HasOne(x => x.Candidate)
            .WithMany()
            .HasForeignKey(x => x.CandidateId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Stage)
            .WithMany(x => x.CandidateApplications)
            .HasForeignKey(x => x.StageId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.CandidateId, x.VacancyId }).IsUnique();
    }
}
