using Cheetah.Blazor.Components.Crud;
using Crm.Recruitment.Contracts.Response;

namespace Crm.Recruitment.Frontend.Models;

public sealed class VacancyGridViewModel : IGridViewModel
{
    public Guid Id { get; init; }

    [GridColumn(DisplayName = "Name", Order = 1)]
    public string Name { get; init; } = "";

    [GridColumn(DisplayName = "State", Order = 2)]
    public string? StateName { get; init; }

    [GridColumn(DisplayName = "Customer", Order = 3)]
    public string? CustomerName { get; init; }

    [GridColumn(DisplayName = "Position", Order = 4)]
    public string? PositionName { get; init; }

    [GridColumn(DisplayName = "Stack", Order = 5)]
    public string? StackItemName { get; init; }

    [GridColumn(DisplayName = "Work Format", Order = 6)]
    public string? WorkFormatName { get; init; }

    [GridColumn(DisplayName = "Description", Order = 7)]
    public string? Description { get; init; }

    public static VacancyGridViewModel FromResponse(VacancyViewModel response) => new()
    {
        Id = response.Id,
        Name = response.Name,
        Description = response.Description,
        StateName = response.StateName,
        CustomerName = response.CustomerName,
        PositionName = response.PositionName,
        StackItemName = response.StackItemName,
        WorkFormatName = response.WorkFormatName
    };
}
