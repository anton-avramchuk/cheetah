using Cheetah.Core.EntityFramework.Configuration;
using Crm.VacancyTasks.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.VacancyTasks.DataAccess.Configurations;

public class VacancyTaskConfigurationOptions : AggregateRootConfigurationOptions<VacancyTask, Guid>
{
    public override string Schema => "vacancytasks";
}

public class VacancyTaskConfiguration : AggregateRootConfiguration<VacancyTask, Guid, VacancyTaskConfigurationOptions>
{
    protected override VacancyTaskConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<VacancyTask> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Description)
            .HasMaxLength(1024);

        builder.Property(x => x.VacancyId)
            .IsRequired();

        builder.HasIndex(x => x.VacancyId);

        builder.Property(x => x.Order)
            .IsRequired();

        builder.Property(x => x.Number)
            .IsRequired();

        builder.HasIndex(x => new { x.VacancyId, x.Number })
            .IsUnique();

        builder.HasOne(x => x.State)
            .WithMany(x => x.VacancyTasks)
            .HasForeignKey(x => x.StateId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Priority)
            .WithMany(x => x.VacancyTasks)
            .HasForeignKey(x => x.PriorityId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.AssigneeId);
    }
}
