using Cheetah.Modules.SalesDocuments.Domain.Entities;
using Cheetah.Modules.SalesDocuments.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.SalesDocuments.Infrastructure.Persistence.Configurations;

/// <summary>
/// Абстрактная базовая EF-конфигурация документа: ключ, игнор доменных событий, общие колонки/индексы,
/// связь со строками (каскадное удаление, авто-include). Наследник добавляет свои поля/индексы через
/// <see cref="ConfigureCustom"/> — точка расширения схемы.
/// </summary>
public abstract class SalesDocumentConfigurationBase<TDoc> : IEntityTypeConfiguration<TDoc>
    where TDoc : SalesDocumentBase
{
    protected virtual string TableName => SalesDocumentsConstants.DefaultDocumentsTableName;
    protected virtual string Schema => SalesDocumentsConstants.DefaultSchema;

    public virtual void Configure(EntityTypeBuilder<TDoc> builder)
    {
        builder.ToTable(TableName, Schema);
        builder.Ignore(e => e.DomainEvents);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DocType).HasConversion<int>();
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.Number).HasMaxLength(SalesDocumentsConstants.MaxNumberLength);
        builder.Property(x => x.Currency).HasMaxLength(SalesDocumentsConstants.MaxCurrencyLength).IsRequired();
        builder.Property(x => x.Subtotal).HasColumnType("numeric(18,2)");
        builder.Property(x => x.DiscountTotal).HasColumnType("numeric(18,2)");
        builder.Property(x => x.TaxTotal).HasColumnType("numeric(18,2)");
        builder.Property(x => x.GrandTotal).HasColumnType("numeric(18,2)");
        builder.Property(x => x.Attributes).HasColumnType("jsonb");

        builder.HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey(l => l.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Lines)
            .HasField("_lines")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();

        builder.HasIndex(x => new { x.DocType, x.Number }).IsUnique();
        builder.HasIndex(x => new { x.CustomerId, x.DocType });
        builder.HasIndex(x => new { x.DocType, x.Status });

        ConfigureCustom(builder);
    }

    /// <summary>Hook для доп. полей/индексов наследника.</summary>
    protected virtual void ConfigureCustom(EntityTypeBuilder<TDoc> builder)
    {
    }
}

/// <summary>Конкретная EF-конфигурация строки документа. Вычисляемые суммы игнорируются.</summary>
public sealed class SalesDocumentLineConfiguration : IEntityTypeConfiguration<SalesDocumentLine>
{
    public void Configure(EntityTypeBuilder<SalesDocumentLine> builder)
    {
        builder.ToTable(SalesDocumentsConstants.DefaultLinesTableName, SalesDocumentsConstants.DefaultSchema);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(SalesDocumentsConstants.MaxNameLength).IsRequired();
        builder.Property(x => x.UnitPrice).HasColumnType("numeric(18,4)");
        builder.Property(x => x.Qty).HasColumnType("numeric(18,4)");
        builder.Property(x => x.DiscountPercent).HasColumnType("numeric(5,2)");
        builder.Property(x => x.TaxRate).HasColumnType("numeric(5,2)");

        builder.Ignore(x => x.LineSubtotal);
        builder.Ignore(x => x.DiscountAmount);
        builder.Ignore(x => x.TaxAmount);
        builder.Ignore(x => x.LineTotal);

        builder.HasIndex(x => x.DocumentId);
        builder.HasIndex(x => x.ProductId);
    }
}
