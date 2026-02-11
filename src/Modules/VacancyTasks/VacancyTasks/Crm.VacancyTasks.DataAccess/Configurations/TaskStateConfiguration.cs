using Cheetah.Core.EntityFramework.Configuration;
using Crm.VacancyTasks.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.VacancyTasks.DataAccess.Configurations;

public class TaskStateConfigurationOptions : EntityConfigurationOptions<TaskState, Guid>
{
    public override string Schema => "vacancytasks";
}

public class TaskStateConfiguration : EntityConfiguration<TaskState, Guid, TaskStateConfigurationOptions>
{
    protected override TaskStateConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<TaskState> builder)
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

        builder.Property(x => x.IsDefault)
            .IsRequired();

        builder.Navigation(x => x.VacancyTasks)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
