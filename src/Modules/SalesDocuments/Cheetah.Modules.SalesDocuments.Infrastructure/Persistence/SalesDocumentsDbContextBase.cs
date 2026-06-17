using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Core.EntityFramework;
using Cheetah.Modules.SalesDocuments.Domain.Entities;
using Cheetah.Modules.SalesDocuments.Infrastructure.Persistence.Configurations;
using Cheetah.Modules.SalesDocuments.Shared;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.SalesDocuments.Infrastructure.Persistence;

/// <summary>
/// Абстрактный generic-DbContext шаблонного модуля SalesDocuments. Хостит документы
/// (<typeparamref name="TDoc"/>) и их строки в одной БД. Наследник закрывает его конкретным типом:
/// <c>class AppSalesDocumentsDbContext : SalesDocumentsDbContextBase&lt;AppSalesDocumentsDbContext, SalesDocument&gt;</c>
/// и поставляет конфигурацию документа через <see cref="CreateDocumentConfiguration"/>. Миграции — у
/// наследника. Имя подключения по умолчанию — <see cref="SalesDocumentsConstants.ConnectionStringName"/>.
/// </summary>
[ConnectionStringName(SalesDocumentsConstants.ConnectionStringName)]
public abstract class SalesDocumentsDbContextBase<TContext, TDoc> : CrmDbContext<TContext>
    where TContext : DbContext
    where TDoc : SalesDocumentBase
{
    public DbSet<TDoc> Documents => Set<TDoc>();
    public DbSet<SalesDocumentLine> Lines => Set<SalesDocumentLine>();

    protected SalesDocumentsDbContextBase(DbContextOptions<TContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(CreateDocumentConfiguration());
        modelBuilder.ApplyConfiguration(new SalesDocumentLineConfiguration());
    }

    /// <summary>Конкретная конфигурация сущности документа, поставляемая наследником.</summary>
    protected abstract IEntityTypeConfiguration<TDoc> CreateDocumentConfiguration();
}
