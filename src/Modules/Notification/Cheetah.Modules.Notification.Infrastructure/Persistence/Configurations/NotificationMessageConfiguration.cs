using Cheetah.Modules.Notification.Domain.Entities;
using Cheetah.Modules.Notification.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Notification.Infrastructure.Persistence.Configurations;

public class NotificationMessageConfiguration : IEntityTypeConfiguration<NotificationMessage>
{
    public void Configure(EntityTypeBuilder<NotificationMessage> builder)
    {
        builder.ToTable("Notifications", "notification");
        builder.Ignore(e => e.DomainEvents);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever(); // Id = NotificationId из события
        builder.Property(x => x.TemplateKey).HasMaxLength(NotificationConstants.MaxTemplateKeyLength).IsRequired();
        builder.Property(x => x.Category).HasMaxLength(NotificationConstants.MaxCategoryLength).IsRequired();
        builder.HasIndex(x => x.RecipientUserId);
    }
}
