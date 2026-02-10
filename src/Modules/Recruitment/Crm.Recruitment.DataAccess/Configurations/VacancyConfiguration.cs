using Cheetah.Core.EntityFramework.Configuration;
using Crm.Recruitment.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Recruitment.DataAccess.Configurations;

public class VacancyConfigurationOptions : AggregateRootConfigurationOptions<Vacancy, Guid>
{
    public override string Schema => "recruitment";
}

public class VacancyConfiguration : AggregateRootConfiguration<Vacancy, Guid, VacancyConfigurationOptions>
{
    protected override VacancyConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<Vacancy> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Description)
            .HasMaxLength(1024);

        builder.HasIndex(x => x.Name).IsUnique();

        builder.HasOne(x => x.State)
            .WithMany(x => x.Vacancies)
            .HasForeignKey(x => x.StateId)
            .IsRequired(false);

        builder.HasOne(x => x.Customer)
            .WithMany(x => x.Vacancies)
            .HasForeignKey(x => x.CustomerId)
            .IsRequired(false);

        builder.HasOne(x => x.Position)
            .WithMany(x => x.Vacancies)
            .HasForeignKey(x => x.PositionId)
            .IsRequired(false);

        builder.HasOne(x => x.StackItem)
            .WithMany(x => x.Vacancies)
            .HasForeignKey(x => x.StackItemId)
            .IsRequired(false);

        builder.HasOne(x => x.WorkFormat)
            .WithMany(x => x.Vacancies)
            .HasForeignKey(x => x.WorkFormatId)
            .IsRequired(false);
    }
}