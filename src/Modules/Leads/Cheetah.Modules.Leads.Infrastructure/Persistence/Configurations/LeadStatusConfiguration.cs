using Cheetah.Modules.Leads.Domain.Entities;
using Cheetah.Modules.Leads.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Leads.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF-конфигурация справочника статусов лида + seed известных статусов (well-known идентификаторы из
/// <see cref="LeadWellKnownIds"/>). Seed попадает в миграцию наследника.
/// </summary>
public sealed class LeadStatusConfiguration : IEntityTypeConfiguration<LeadStatus>
{
    public void Configure(EntityTypeBuilder<LeadStatus> builder)
    {
        builder.ToTable(LeadsConstants.DefaultStatusesTableName, LeadsConstants.DefaultSchema);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(LeadsConstants.MaxCodeLength).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(LeadsConstants.MaxNameLength).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();

        builder.HasData(
            Seed(LeadWellKnownIds.StatusNew, LeadStatusCodes.New, "Новый", 1, false),
            Seed(LeadWellKnownIds.StatusWorking, LeadStatusCodes.Working, "В работе", 2, false),
            Seed(LeadWellKnownIds.StatusQualified, LeadStatusCodes.Qualified, "Квалифицирован", 3, false),
            Seed(LeadWellKnownIds.StatusConverted, LeadStatusCodes.Converted, "Сконвертирован", 4, true),
            Seed(LeadWellKnownIds.StatusDisqualified, LeadStatusCodes.Disqualified, "Дисквалифицирован", 5, true));
    }

    // Анонимный объект — seed без публичного конструктора сущности.
    private static object Seed(Guid id, string code, string name, int order, bool isTerminal)
        => new { Id = id, Code = code, Name = name, Order = order, IsTerminal = isTerminal, IsActive = true };
}
