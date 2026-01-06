namespace Cheetah.Identity.Contracts.ViewModels;

/// <summary>
/// Claim (permission or attribute) view model
/// </summary>
public class ClaimViewModel
{
    public Guid Id { get; set; }
    public string ClaimType { get; set; } = null!;
    public string ClaimValue { get; set; } = null!;
}
