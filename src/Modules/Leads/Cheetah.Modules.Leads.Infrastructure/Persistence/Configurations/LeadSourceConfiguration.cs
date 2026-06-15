using Cheetah.Modules.Leads.Domain.Entities;
using Cheetah.Modules.Leads.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Leads.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF-конфигурация справочника источников лида + seed известных источников (well-known идентификаторы
/// из <see cref="LeadWellKnownIds"/>). Seed попадает в миграцию наследника.
/// </summary>
public sealed class LeadSourceConfiguration : IEntityTypeConfiguration<LeadSource>
{
    public void Configure(EntityTypeBuilder<LeadSource> builder)
    {
        builder.ToTable(LeadsConstants.DefaultSourcesTableName, LeadsConstants.DefaultSchema);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(LeadsConstants.MaxCodeLength).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(LeadsConstants.MaxNameLength).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();

        builder.HasData(
            Seed(LeadWellKnownIds.SourceWeb, LeadSourceCodes.Web, "Сайт", 1),
            Seed(LeadWellKnownIds.SourceImport, LeadSourceCodes.Import, "Импорт", 2),
            Seed(LeadWellKnownIds.SourceAds, LeadSourceCodes.Ads, "Реклама", 3),
            Seed(LeadWellKnownIds.SourceReferral, LeadSourceCodes.Referral, "Рекомендация", 4),
            Seed(LeadWellKnownIds.SourceManual, LeadSourceCodes.Manual, "Вручную", 5),
            Seed(LeadWellKnownIds.SourceApi, LeadSourceCodes.Api, "API", 6));
    }

    private static object Seed(Guid id, string code, string name, int order)
        => new { Id = id, Code = code, Name = name, Order = order, IsActive = true };
}
