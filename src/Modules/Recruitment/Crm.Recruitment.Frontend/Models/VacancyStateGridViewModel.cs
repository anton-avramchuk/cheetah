using Cheetah.Blazor.Components.Crud;
using Crm.Recruitment.Contracts.Response;

namespace Crm.Recruitment.Frontend.Models;

public sealed class VacancyStateGridViewModel : IGridViewModel
{
    public Guid Id { get; init; }

    [GridColumn(DisplayName = "Name", Order = 1)]
    public string Name { get; init; } = "";

    

    [GridColumn(DisplayName = "Color", Order = 3, Template = GridColumnTemplate.Color)]
    public string Color { get; init; } = "";

    

    public static VacancyStateGridViewModel FromResponse(VacancyStateViewModel response) => new()
    {
        Id = response.Id,
        Name = response.Name,
        Color = response.Color ?? "",
    };
}