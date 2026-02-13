using Cheetah.Blazor.Components.Crud;
using Crm.Recruitment.Contracts.Response;

namespace Crm.Recruitment.Frontend.Models;

public sealed class CustomerDirectionGridViewModel : IGridViewModel
{
    public Guid Id { get; init; }

    [GridColumn(DisplayName = "Name", Order = 1)]
    public string Name { get; init; } = "";

    [GridColumn(DisplayName = "Description", Order = 2)]
    public string? Description { get; init; }

    public static CustomerDirectionGridViewModel FromResponse(CustomerDirectionViewModel response) => new()
    {
        Id = response.Id,
        Name = response.Name,
        Description = response.Description
    };
}
