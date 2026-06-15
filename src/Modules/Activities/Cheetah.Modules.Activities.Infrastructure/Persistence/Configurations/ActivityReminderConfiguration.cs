using Cheetah.Modules.Activities.Domain.Entities;
using Cheetah.Modules.Activities.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Activities.Infrastructure.Persistence.Configurations;

/// <summary>EF-конфигурация напоминания (дитя активности). Конкретный тип — без наследования.</summary>
public sealed class ActivityReminderConfiguration : IEntityTypeConfiguration<ActivityReminder>
{
    public void Configure(EntityTypeBuilder<ActivityReminder> builder)
    {
        builder.ToTable(ActivitiesConstants.DefaultRemindersTableName, ActivitiesConstants.DefaultSchema);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Channel).HasMaxLength(ActivitiesConstants.MaxChannelLength).IsRequired();

        builder.HasIndex(x => x.ActivityId);
        builder.HasIndex(x => new { x.Sent });
    }
}
