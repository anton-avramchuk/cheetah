using System;
using Cheetah.Blazor.Components.Crud;
using Crm.Identity.Contracts.Response;

namespace Crm.Identity.Frontend.Models;

public sealed class RoleGridViewModel : IGridViewModel
{
    public Guid Id { get; init; }

    [GridColumn(DisplayName = "Name", Order = 1)]
    public string Name { get; init; } = "";

    public static RoleGridViewModel FromResponse(RoleViewModel response) => new()
    {
        Id = response.Id,
        Name = response.Name
    };
}
