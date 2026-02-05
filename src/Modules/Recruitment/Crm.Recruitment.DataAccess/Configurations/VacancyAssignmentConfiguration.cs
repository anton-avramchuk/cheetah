using Cheetah.Core.EntityFramework.Configuration;
using Crm.Recruitment.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Recruitment.DataAccess.Configurations;

public class VacancyAssignmentConfigurationOptions : EntityConfigurationOptions<VacancyAssignment, Guid>
{
    public override string Schema => "recruitment";
}

public class VacancyAssignmentConfiguration : EntityConfiguration<VacancyAssignment, Guid, VacancyAssignmentConfigurationOptions>
{
    protected override VacancyAssignmentConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<VacancyAssignment> builder)
    {
        base.Configure(builder);

        builder.HasOne(x => x.Vacancy)
            .WithMany()
            .HasForeignKey(x => x.VacancyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Role)
            .WithMany(x => x.Assignments)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Prevent duplicate assignments (same user, same role, same vacancy)
        builder.HasIndex(x => new { x.VacancyId, x.UserId, x.RoleId })
            .IsUnique();
    }
}
