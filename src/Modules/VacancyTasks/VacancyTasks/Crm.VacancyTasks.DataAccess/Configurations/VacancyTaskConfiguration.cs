using Cheetah.Core.EntityFramework.Configuration;
using Crm.VacancyTasks.Domain;
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

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Description)
            .HasMaxLength(1024);

        builder.HasIndex(x => x.Name).IsUnique();
    }
}