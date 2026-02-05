using Cheetah.Core.EntityFramework.Configuration;
using Crm.Recruitment.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Recruitment.DataAccess.Configurations;

public class VacancyStateConfigurationOptions : EntityConfigurationOptions<VacancyState, Guid>
{
    public override string Schema => "recruitment";
}

public class VacancyStateConfiguration : EntityConfiguration<VacancyState, Guid, VacancyStateConfigurationOptions>
{
    protected override VacancyStateConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<VacancyState> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200)
            ;

        builder.HasIndex(x => x.Name).IsUnique();

        builder.Property(x => x.Order)
            .IsRequired();

        builder.Navigation(x => x.Vacancies)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}