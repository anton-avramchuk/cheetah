using Cheetah.Blazor.Components.Crud;
using __Prefix__.ModuleName.Contracts.Response;

namespace __Prefix__.ModuleName.Frontend.Models;

/// <summary>
/// Grid view model for SampleEntity list.
/// </summary>
public sealed class SampleEntityGridViewModel : IGridViewModel
{
    public Guid Id { get; init; }

    [GridColumn(DisplayName = "Name", Order = 1)]
    public string Name { get; init; } = "";

    [GridColumn(DisplayName = "Description", Order = 2)]
    public string? Description { get; init; }

    public static SampleEntityGridViewModel FromResponse(SampleEntityViewModel response) => new()
    {
        Id = response.Id,
        Name = response.Name,
        Description = response.Description
    };
}
