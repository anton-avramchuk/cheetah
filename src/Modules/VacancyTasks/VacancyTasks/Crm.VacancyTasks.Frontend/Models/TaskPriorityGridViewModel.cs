using Cheetah.Blazor.Components.Crud;
using Crm.VacancyTasks.Contracts.Response;

namespace Crm.VacancyTasks.Frontend.Models;

public sealed class TaskPriorityGridViewModel : IGridViewModel
{
    public Guid Id { get; init; }

    [GridColumn(DisplayName = "Name", Order = 1)]
    public string Name { get; init; } = "";

    [GridColumn(DisplayName = "Order", Order = 2)]
    public int Order { get; init; }

    [GridColumn(DisplayName = "Color", Order = 3, Template = GridColumnTemplate.Color)]
    public string? Color { get; init; }

    public static TaskPriorityGridViewModel FromResponse(TaskPriorityViewModel response) => new()
    {
        Id = response.Id,
        Name = response.Name,
        Order = response.Order,
        Color = response.Color
    };
}
