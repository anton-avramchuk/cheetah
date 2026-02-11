using Cheetah.Core.EntityFramework.Configuration;
using Crm.VacancyTasks.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.VacancyTasks.DataAccess.Configurations;

public class TaskPriorityConfigurationOptions : EntityConfigurationOptions<TaskPriority, Guid>
{
    public override string Schema => "vacancytasks";
}

public class TaskPriorityConfiguration : EntityConfiguration<TaskPriority, Guid, TaskPriorityConfigurationOptions>
{
    protected override TaskPriorityConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<TaskPriority> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(x => x.Name).IsUnique();

        builder.Property(x => x.Order)
            .IsRequired();

        builder.Property(x => x.Color)
            .HasMaxLength(7);

        builder.Navigation(x => x.VacancyTasks)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
