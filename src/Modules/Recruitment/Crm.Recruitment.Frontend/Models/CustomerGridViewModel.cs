using Cheetah.Blazor.Components.Crud;
using Crm.Recruitment.Contracts.Response;

namespace Crm.Recruitment.Frontend.Models;

public sealed class CustomerGridViewModel : IGridViewModel
{
    public Guid Id { get; init; }

    [GridColumn(DisplayName = "Name", Order = 1)]
    public string Name { get; init; } = "";

    [GridColumn(DisplayName = "Code", Order = 2)]
    public string? Code { get; init; }

    public static CustomerGridViewModel FromResponse(CustomerViewModel response) => new()
    {
        Id = response.Id,
        Name = response.Name,
        Code = response.Code
    };
}
