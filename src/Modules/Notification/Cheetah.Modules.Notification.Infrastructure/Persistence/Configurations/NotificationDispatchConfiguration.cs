using Cheetah.Modules.Notification.Domain.Entities;
using Cheetah.Modules.Notification.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Notification.Infrastructure.Persistence.Configurations;

public class NotificationDispatchConfiguration : IEntityTypeConfiguration<NotificationDispatch>
{
    public void Configure(EntityTypeBuilder<NotificationDispatch> builder)
    {
        builder.ToTable("Dispatches", "notification");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Channel).HasMaxLength(NotificationConstants.MaxChannelLength).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(NotificationConstants.MaxChannelLength).IsRequired();
        builder.Property(x => x.Error).HasMaxLength(NotificationConstants.MaxErrorLength);
        builder.HasIndex(x => x.NotificationId);
        builder.HasIndex(x => x.Status);
    }
}
