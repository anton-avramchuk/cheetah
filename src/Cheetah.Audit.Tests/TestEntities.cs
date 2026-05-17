using Cheetah.Audit;

namespace Cheetah.Audit.Tests;

[Auditable]
public class TestCustomer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string? Email { get; set; }

    [Sensitive]
    public string? PasswordHash { get; set; }

    [NotAudited]
    public int LoginCount { get; set; }
}

public class TestNonAudited
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "";
}
