using Cheetah.Core.Domain;

namespace Cheetah.Modules.Email.Domain.Entities;

/// <summary>
/// Маркер отправленного письма. <see cref="Entity{TId}.Id"/> = DispatchId — обеспечивает
/// провайдер-уровневую идемпотентность: повтор EmailRequested с тем же DispatchId не приводит
/// к повторной отправке (вторая линия защиты помимо дедупа события на шине).
/// </summary>
public class SentEmail : Entity<Guid>, ICreateAtEntity
{
    public Guid NotificationId { get; private set; }
    public string ToAddress { get; private set; } = null!;
    public bool Succeeded { get; private set; }
    public string? ProviderMessageId { get; private set; }
    public string? Error { get; private set; }

    public DateTimeOffset? CreatedAt { get; set; }

    private SentEmail() { } // EF

    public static SentEmail Create(Guid dispatchId, Guid notificationId, string toAddress)
    {
        if (dispatchId == Guid.Empty)
            throw new ArgumentException("DispatchId cannot be empty", nameof(dispatchId));
        ArgumentException.ThrowIfNullOrWhiteSpace(toAddress);

        return new SentEmail
        {
            Id = dispatchId,
            NotificationId = notificationId,
            ToAddress = toAddress
        };
    }

    public void MarkSucceeded(string? providerMessageId)
    {
        Succeeded = true;
        ProviderMessageId = providerMessageId;
        Error = null;
    }

    public void MarkFailed(string? error)
    {
        Succeeded = false;
        Error = error;
    }
}
