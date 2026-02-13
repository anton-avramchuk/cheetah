using Cheetah.Blazor.Components.Crud;
using Crm.Recruitment.Contracts.Response;

namespace Crm.Recruitment.Frontend.Models;

public sealed class StackItemGridViewModel : IGridViewModel
{
    public Guid Id { get; init; }

    [GridColumn(DisplayName = "Name", Order = 1)]
    public string Name { get; init; } = "";

    public static StackItemGridViewModel FromResponse(StackItemViewModel response) => new()
    {
        Id = response.Id,
        Name = response.Name
    };
}
