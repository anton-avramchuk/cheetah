using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Core.EntityFramework;
using Cheetah.Modules.Leads.Domain.Entities;
using Cheetah.Modules.Leads.Infrastructure.Persistence.Configurations;
using Cheetah.Modules.Leads.Shared;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.Leads.Infrastructure.Persistence;

/// <summary>
/// Абстрактный generic-DbContext шаблонного модуля Leads. Хостит лиды (<typeparamref name="TLead"/>) и
/// справочники статусов/источников (концертные lookup-таблицы с seed). Наследник закрывает его
/// конкретным типом лида: <c>class AppLeadsDbContext : LeadsDbContextBase&lt;AppLeadsDbContext, Lead&gt;</c>
/// и поставляет конфигурацию лида через <see cref="CreateLeadConfiguration"/>. Миграции — у наследника.
/// </summary>
[ConnectionStringName(LeadsConstants.ConnectionStringName)]
public abstract class LeadsDbContextBase<TContext, TLead> : CrmDbContext<TContext>
    where TContext : DbContext
    where TLead : LeadBase
{
    public DbSet<TLead> Leads => Set<TLead>();
    public DbSet<LeadStatus> LeadStatuses => Set<LeadStatus>();
    public DbSet<LeadSource> LeadSources => Set<LeadSource>();

    protected LeadsDbContextBase(DbContextOptions<TContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new LeadStatusConfiguration());
        modelBuilder.ApplyConfiguration(new LeadSourceConfiguration());
        modelBuilder.ApplyConfiguration(CreateLeadConfiguration());
    }

    /// <summary>Конкретная конфигурация сущности лида, поставляемая наследником.</summary>
    protected abstract IEntityTypeConfiguration<TLead> CreateLeadConfiguration();
}
