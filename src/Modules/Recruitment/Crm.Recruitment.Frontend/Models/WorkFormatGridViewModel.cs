using Cheetah.Blazor.Components.Crud;
using Crm.Recruitment.Contracts.Response;

namespace Crm.Recruitment.Frontend.Models;

public sealed class WorkFormatGridViewModel : IGridViewModel
{
    public Guid Id { get; init; }

    [GridColumn(DisplayName = "Name", Order = 1)]
    public string Name { get; init; } = "";

    public static WorkFormatGridViewModel FromResponse(WorkFormatViewModel response) => new()
    {
        Id = response.Id,
        Name = response.Name
    };
}
