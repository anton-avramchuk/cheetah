using Cheetah.Admin.Modules.Clients.Contracts.Response;
using Cheetah.Blazor.Components.Crud;

namespace Cheetah.Admin.Modules.Clients.Frontend.Models;

public sealed class TariffGridViewModel : IGridViewModel
{
    public Guid Id { get; init; }

    [GridColumn(DisplayName = "Name", Order = 1)]
    public string Name { get; init; } = "";

    [GridColumn(DisplayName = "Description", Order = 2)]
    public string? Description { get; init; }

    [GridColumn(DisplayName = "Price", Order = 3, Format = "N2")]
    public decimal Price { get; init; }

    [GridColumn(DisplayName = "Currency", Order = 4)]
    public string Currency { get; init; } = "";

    [GridColumn(DisplayName = "Active", Order = 5)]
    public bool IsActive { get; init; }

    public static TariffGridViewModel FromResponse(TariffViewModel response) => new()
    {
        Id = response.Id,
        Name = response.Name,
        Description = response.Description,
        Price = response.Price,
        Currency = response.Currency,
        IsActive = response.IsActive
    };
}
