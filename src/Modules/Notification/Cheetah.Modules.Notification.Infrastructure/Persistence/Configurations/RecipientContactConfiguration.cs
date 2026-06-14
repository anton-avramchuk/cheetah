using Cheetah.Modules.Notification.Domain.Entities;
using Cheetah.Modules.Notification.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Notification.Infrastructure.Persistence.Configurations;

public class RecipientContactConfiguration : IEntityTypeConfiguration<RecipientContact>
{
    public void Configure(EntityTypeBuilder<RecipientContact> builder)
    {
        builder.ToTable("RecipientContacts", "notification");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever(); // Id = UserId из Identity
        builder.Property(x => x.Email).HasMaxLength(NotificationConstants.MaxContactValueLength);
        builder.Property(x => x.Phone).HasMaxLength(NotificationConstants.MaxContactValueLength);
        builder.Property(x => x.PushToken).HasMaxLength(NotificationConstants.MaxContactValueLength);
    }
}
