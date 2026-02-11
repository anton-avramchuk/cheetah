using System.ComponentModel.DataAnnotations;

namespace Crm.Candidates.Frontend.Models;

/// <summary>
/// Form model for creating/editing a Candidate.
/// </summary>
public class CandidateFormModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "First name is required")]
    [StringLength(256, ErrorMessage = "First name must not exceed 256 characters")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(256, ErrorMessage = "Last name must not exceed 256 characters")]
    public string LastName { get; set; } = string.Empty;

    [StringLength(256, ErrorMessage = "Email must not exceed 256 characters")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string? Email { get; set; }

    [StringLength(50, ErrorMessage = "Phone must not exceed 50 characters")]
    public string? Phone { get; set; }

    [StringLength(256, ErrorMessage = "City must not exceed 256 characters")]
    public string? City { get; set; }

    [StringLength(256, ErrorMessage = "Position must not exceed 256 characters")]
    public string? CurrentPosition { get; set; }

    [StringLength(256, ErrorMessage = "Company must not exceed 256 characters")]
    public string? CurrentCompany { get; set; }

    public decimal? SalaryExpectation { get; set; }

    [StringLength(4000, ErrorMessage = "About must not exceed 4000 characters")]
    public string? About { get; set; }

    public bool IsEdit => Id.HasValue;
}
