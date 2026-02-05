using Cheetah.Core.Domain;

namespace Crm.Recruitment.Domain;

/// <summary>
/// Local copy of user from Identity module.
/// Synchronized via domain events.
/// </summary>
public class User : Entity<Guid>
{
    private User()
    {
    }

    public string DisplayName { get; private set; } = null!;

    public string? Email { get; private set; }

    public bool IsActive { get; private set; } = true;

    public static User Create(Guid id, string displayName, string? email = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        return new User
        {
            Id = id,
            DisplayName = displayName,
            Email = email,
            IsActive = true
        };
    }

    public void Update(string displayName, string? email = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        DisplayName = displayName;
        Email = email;
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}
