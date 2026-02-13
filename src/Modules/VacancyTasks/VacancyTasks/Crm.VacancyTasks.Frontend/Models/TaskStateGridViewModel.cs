using Cheetah.Blazor.Components.Crud;
using Crm.VacancyTasks.Contracts.Response;

namespace Crm.VacancyTasks.Frontend.Models;

public sealed class TaskStateGridViewModel : IGridViewModel
{
    public Guid Id { get; init; }

    [GridColumn(DisplayName = "Name", Order = 1)]
    public string Name { get; init; } = "";

    [GridColumn(DisplayName = "Order", Order = 2)]
    public int Order { get; init; }

    [GridColumn(DisplayName = "Color", Order = 3)]
    public string? Color { get; init; }

    [GridColumn(DisplayName = "Is Default", Order = 4)]
    public bool IsDefault { get; init; }

    public static TaskStateGridViewModel FromResponse(TaskStateViewModel response) => new()
    {
        Id = response.Id,
        Name = response.Name,
        Order = response.Order,
        Color = response.Color,
        IsDefault = response.IsDefault
    };
}
