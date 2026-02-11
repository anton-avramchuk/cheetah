using Cheetah.Blazor.Components.Crud;
using Crm.VacancyTasks.Contracts.Response;

namespace Crm.VacancyTasks.Frontend.Models;

/// <summary>
/// Grid view model for VacancyTask list.
/// </summary>
public sealed class VacancyTaskGridViewModel : IGridViewModel
{
    public Guid Id { get; init; }

    [GridColumn(DisplayName = "Title", Order = 1)]
    public string Title { get; init; } = "";

    [GridColumn(DisplayName = "Description", Order = 2)]
    public string? Description { get; init; }

    public static VacancyTaskGridViewModel FromResponse(VacancyTaskViewModel response) => new()
    {
        Id = response.Id,
        Title = response.Title,
        Description = response.Description
    };
}