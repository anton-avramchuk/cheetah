using Cheetah.Admin.Modules.Clients.Contracts.Response;
using Cheetah.Blazor.Components.Crud;

namespace Cheetah.Admin.Modules.Clients.Frontend.Models;

/// <summary>
/// Grid view model for clients list.
/// </summary>
public sealed class ClientGridViewModel : IGridViewModel
{
    public Guid Id { get; init; }

    [GridColumn(DisplayName = "Название", Order = 1)]
    public string Name { get; init; } = "";

    [GridColumn(DisplayName = "Описание", Order = 2)]
    public string? Description { get; init; }

    [GridColumn(DisplayName = "Тенант", Order = 3)]
    public string? TenantName { get; init; }
}
