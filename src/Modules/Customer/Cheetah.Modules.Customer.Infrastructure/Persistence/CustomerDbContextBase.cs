using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Core.EntityFramework;
using Cheetah.Modules.Customer.Domain.Entities;
using Cheetah.Modules.Customer.Shared;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.Customer.Infrastructure.Persistence;

/// <summary>
/// Абстрактный generic-DbContext шаблонного модуля. Хостит оба агрегата модуля в одной БД:
/// клиентов (<typeparamref name="TCustomer"/>) и их контактные лица (<typeparamref name="TContact"/>).
/// Наследник закрывает его конкретными типами:
/// <c>class AppCustomerDbContext : CustomerDbContextBase&lt;AppCustomerDbContext, Customer, Contact&gt;</c>
/// и поставляет конфигурации сущностей через <see cref="CreateCustomerConfiguration"/> и
/// <see cref="CreateContactConfiguration"/>. Имя подключения по умолчанию —
/// <see cref="CustomerConstants.ConnectionStringName"/> (наследник может переопределить своим
/// <c>[ConnectionStringName]</c>).
/// </summary>
[ConnectionStringName(CustomerConstants.ConnectionStringName)]
public abstract class CustomerDbContextBase<TContext, TCustomer, TContact> : CrmDbContext<TContext>
    where TContext : DbContext
    where TCustomer : CustomerBase
    where TContact : ContactBase
{
    public DbSet<TCustomer> Customers => Set<TCustomer>();
    public DbSet<TContact> Contacts => Set<TContact>();

    protected CustomerDbContextBase(DbContextOptions<TContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(CreateCustomerConfiguration());
        modelBuilder.ApplyConfiguration(CreateContactConfiguration());
    }

    /// <summary>Конкретная конфигурация сущности клиента, поставляемая наследником.</summary>
    protected abstract IEntityTypeConfiguration<TCustomer> CreateCustomerConfiguration();

    /// <summary>Конкретная конфигурация сущности контактного лица, поставляемая наследником.</summary>
    protected abstract IEntityTypeConfiguration<TContact> CreateContactConfiguration();
}
