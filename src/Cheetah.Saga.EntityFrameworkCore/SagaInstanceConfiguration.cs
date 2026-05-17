using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Saga.EntityFrameworkCore;

public class SagaInstanceConfiguration : IEntityTypeConfiguration<SagaInstance>
{
    public void Configure(EntityTypeBuilder<SagaInstance> builder)
    {
        builder.ToTable("SagaInstances");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SagaType).IsRequired().HasMaxLength(512);
        builder.Property(x => x.CorrelationKey).IsRequired().HasMaxLength(256);
        builder.Property(x => x.DataType).IsRequired().HasMaxLength(512);
        builder.Property(x => x.DataJson).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(2000);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        // Concurrency token — EF при UPDATE добавит WHERE Version = @original;
        // если в БД уже другая Version, бросится DbUpdateConcurrencyException.
        builder.Property(x => x.Version).IsConcurrencyToken();

        // Уникальный (SagaType, CorrelationKey) гарантирует одна сага = одна instance.
        builder.HasIndex(x => new { x.SagaType, x.CorrelationKey })
            .IsUnique()
            .HasDatabaseName("UX_SagaInstances_Correlation");
    }
}
