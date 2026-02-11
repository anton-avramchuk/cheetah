using Cheetah.Blazor.Components.Crud;
using Crm.Candidates.Contracts.Response;

namespace Crm.Candidates.Frontend.Models;

/// <summary>
/// Grid view model for Candidate list.
/// </summary>
public sealed class CandidateGridViewModel : IGridViewModel
{
    public Guid Id { get; init; }

    [GridColumn(DisplayName = "First Name", Order = 1)]
    public string FirstName { get; init; } = "";

    [GridColumn(DisplayName = "Last Name", Order = 2)]
    public string LastName { get; init; } = "";

    [GridColumn(DisplayName = "Email", Order = 3)]
    public string? Email { get; init; }

    [GridColumn(DisplayName = "Phone", Order = 4)]
    public string? Phone { get; init; }

    [GridColumn(DisplayName = "Current Position", Order = 5)]
    public string? CurrentPosition { get; init; }

    [GridColumn(DisplayName = "Current Company", Order = 6)]
    public string? CurrentCompany { get; init; }

    public static CandidateGridViewModel FromResponse(CandidateViewModel response) => new()
    {
        Id = response.Id,
        FirstName = response.FirstName,
        LastName = response.LastName,
        Email = response.Email,
        Phone = response.Phone,
        CurrentPosition = response.CurrentPosition,
        CurrentCompany = response.CurrentCompany
    };
}
