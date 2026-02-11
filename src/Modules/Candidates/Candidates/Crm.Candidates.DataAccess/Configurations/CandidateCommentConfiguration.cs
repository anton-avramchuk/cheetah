using Cheetah.Core.EntityFramework.Configuration;
using Crm.Candidates.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Candidates.DataAccess.Configurations;

public class CandidateCommentConfigurationOptions : EntityConfigurationOptions<CandidateComment, Guid>
{
    public override string Schema => "candidates";
}

public class CandidateCommentConfiguration : EntityConfiguration<CandidateComment, Guid, CandidateCommentConfigurationOptions>
{
    protected override CandidateCommentConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<CandidateComment> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Text)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(x => x.AuthorId).IsRequired();

        builder.HasOne<Candidate>()
            .WithMany(x => x.Comments)
            .HasForeignKey(x => x.CandidateId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.CandidateId);
    }
}
