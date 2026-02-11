using Cheetah.Blazor.Components.Crud;
using Crm.VacancyTasks.Contracts.Response;

namespace Crm.VacancyTasks.Frontend.Models;

/// <summary>
/// Grid view model for VacancyTask list.
/// </summary>
public sealed class VacancyTaskGridViewModel : IGridViewModel
{
    public Guid Id { get; init; }

    [GridColumn(DisplayName = "Name", Order = 1)]
    public string Name { get; init; } = "";

    [GridColumn(DisplayName = "Description", Order = 2)]
    public string? Description { get; init; }

    public static VacancyTaskGridViewModel FromResponse(VacancyTaskViewModel response) => new()
    {
        Id = response.Id,
        Name = response.Name,
        Description = response.Description
    };
}