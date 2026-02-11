using System.ComponentModel.DataAnnotations;

namespace Crm.Candidates.Frontend.Models;

/// <summary>
/// Form model for creating/editing a Candidate.
/// </summary>
public class CandidateFormModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, ErrorMessage = "Name must not exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description must not exceed 1000 characters")]
    public string Description { get; set; } = string.Empty;

    public bool IsEdit => Id.HasValue;
}