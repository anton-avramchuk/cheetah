using Cheetah.Modules.Email.Domain.Entities;
using Cheetah.Modules.Email.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Email.Infrastructure.Persistence.Configurations;

public class SentEmailConfiguration : IEntityTypeConfiguration<SentEmail>
{
    public void Configure(EntityTypeBuilder<SentEmail> builder)
    {
        builder.ToTable("SentEmails", "email");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever(); // Id = DispatchId
        builder.Property(x => x.ToAddress).HasMaxLength(EmailConstants.MaxAddressLength).IsRequired();
        builder.Property(x => x.ProviderMessageId).HasMaxLength(EmailConstants.MaxProviderMessageIdLength);
        builder.Property(x => x.Error).HasMaxLength(EmailConstants.MaxErrorLength);
        builder.HasIndex(x => x.NotificationId);
    }
}
