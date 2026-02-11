using Cheetah.Blazor.Components.Crud;
using Crm.Candidates.Contracts.Response;

namespace Crm.Candidates.Frontend.Models;

/// <summary>
/// Grid view model for Candidate list.
/// </summary>
public sealed class CandidateGridViewModel : IGridViewModel
{
    public Guid Id { get; init; }

    [GridColumn(DisplayName = "Name", Order = 1)]
    public string Name { get; init; } = "";

    [GridColumn(DisplayName = "Description", Order = 2)]
    public string? Description { get; init; }

    public static CandidateGridViewModel FromResponse(CandidateViewModel response) => new()
    {
        Id = response.Id,
        Name = response.Name,
        Description = response.Description
    };
}