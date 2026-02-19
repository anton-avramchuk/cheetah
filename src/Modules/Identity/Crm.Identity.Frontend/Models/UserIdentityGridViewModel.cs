using Cheetah.Blazor.Components.Crud;
using Crm.Identity.Contracts.Response;

namespace Crm.Identity.Frontend.Models;

/// <summary>
/// Grid view model for UserIdentity list.
/// </summary>
public sealed class UserIdentityGridViewModel : IGridViewModel
{
    public Guid Id { get; init; }

    [GridColumn(DisplayName = "Name", Order = 1)]
    public string Name { get; init; } = "";

    [GridColumn(DisplayName = "Description", Order = 2)]
    public string? Description { get; init; }

    public static UserIdentityGridViewModel FromResponse(UserIdentityViewModel response) => new()
    {
        Id = response.Id,
        Name = response.Name,
        Description = response.Description
    };
}