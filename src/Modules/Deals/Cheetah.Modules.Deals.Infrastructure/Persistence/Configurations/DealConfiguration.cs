using Cheetah.Modules.Deals.Domain.Entities;
using Cheetah.Modules.Deals.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Deals.Infrastructure.Persistence.Configurations;

public class DealConfiguration : IEntityTypeConfiguration<Deal>
{
    public void Configure(EntityTypeBuilder<Deal> builder)
    {
        builder.ToTable("Deals", DealsConstants.Schema);
        builder.HasKey(x => x.Id);
        builder.Ignore(x => x.DomainEvents);

        builder.Property(x => x.Title).HasMaxLength(DealsConstants.MaxTitleLength).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.LostReason).HasMaxLength(DealsConstants.MaxLostReasonLength);

        // Money — owned-type: две колонки Amount/Currency в той же таблице.
        builder.OwnsOne(x => x.Value, v =>
        {
            v.Property(p => p.Amount).HasColumnName("Amount").HasColumnType(DealsConstants.MoneyColumnType);
            v.Property(p => p.Currency).HasColumnName("Currency").HasMaxLength(DealsConstants.MaxCurrencyLength);
        });
        builder.Navigation(x => x.Value).IsRequired();

        builder.HasMany(x => x.History)
            .WithOne()
            .HasForeignKey(h => h.DealId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.History)
            .HasField("_history")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Индексы под горячие пути (план §6.1).
        builder.HasIndex(x => new { x.OwnerId, x.Status });
        builder.HasIndex(x => new { x.PipelineId, x.StageId });
        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.ExpectedCloseDate);
    }
}

public class DealStageHistoryConfiguration : IEntityTypeConfiguration<DealStageHistory>
{
    public void Configure(EntityTypeBuilder<DealStageHistory> builder)
    {
        builder.ToTable("DealStageHistory", DealsConstants.Schema);
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.DealId);
    }
}
