using Cheetah.Audit;

namespace Cheetah.Audit.Integration.Tests;

[Auditable]
public class Customer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string? Email { get; set; }

    [Sensitive]
    public string? PasswordHash { get; set; }
}
