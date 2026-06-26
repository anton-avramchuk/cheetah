using Cheetah.AspNetCore.Blazor.Grid;

namespace Cheetah.Modules.Catalog.Blazor.ViewModels;

/// <summary>«Лёгкий» ViewModel прайс-листа для грида (без строк цен).</summary>
public sealed class PriceListGridViewModel : IHasId
{
    public Guid Id { get; set; }

    [GridColumn("Название", order: 0)]
    public string Name { get; set; } = string.Empty;

    [GridColumn("Валюта", order: 1)]
    public string Currency { get; set; } = string.Empty;

    [GridColumn("По умолчанию", order: 2)]
    public bool IsDefault { get; set; }

    [GridColumn("Действует с", order: 3)]
    public DateTimeOffset? ValidFrom { get; set; }

    [GridColumn("Действует по", order: 4)]
    public DateTimeOffset? ValidTo { get; set; }
}

/// <summary>ViewModel прайс-листа для формы редактирования (валюта менять нельзя).</summary>
public sealed class PriceListDetailsViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public DateTimeOffset? ValidFrom { get; set; }
    public DateTimeOffset? ValidTo { get; set; }
}

/// <summary>ViewModel прайс-листа для формы создания.</summary>
public sealed class PriceListCreateViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Currency { get; set; } = "RUB";
    public bool IsDefault { get; set; }
    public DateTimeOffset? ValidFrom { get; set; }
    public DateTimeOffset? ValidTo { get; set; }
}
