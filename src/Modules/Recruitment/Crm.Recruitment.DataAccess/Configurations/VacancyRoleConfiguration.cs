using Cheetah.Core.EntityFramework.Configuration;
using Crm.Recruitment.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Recruitment.DataAccess.Configurations;

public class VacancyRoleConfigurationOptions : EntityConfigurationOptions<VacancyRole, Guid>
{
    public override string Schema => "recruitment";
}

public class VacancyRoleConfiguration : EntityConfiguration<VacancyRole, Guid, VacancyRoleConfigurationOptions>
{
    protected override VacancyRoleConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<VacancyRole> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.IsSingle)
            .IsRequired();

        builder.Property(x => x.Order)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Navigation(x => x.Assignments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
