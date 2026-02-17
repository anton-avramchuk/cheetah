using Cheetah.Blazor.Components.Crud;
using Crm.Candidates.Contracts.Response;

namespace Crm.Candidates.Frontend.Models;

public sealed class CandidateSourceGridViewModel : IGridViewModel
{
    public Guid Id { get; init; }

    [GridColumn(DisplayName = "Name", Order = 1)]
    public string Name { get; init; } = "";

    [GridColumn(DisplayName = "Order", Order = 2)]
    public int Order { get; init; }

    [GridColumn(DisplayName = "Color", Order = 3, Template = GridColumnTemplate.Color)]
    public string? Color { get; init; }

    public static CandidateSourceGridViewModel FromResponse(CandidateSourceViewModel response) => new()
    {
        Id = response.Id,
        Name = response.Name,
        Order = response.Order,
        Color = response.Color
    };
}
