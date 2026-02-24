using System;
using Cheetah.Blazor.Components.Crud;
using Crm.Identity.Contracts.Response;

namespace Crm.Identity.Frontend.Models;

public sealed class UserGridViewModel : IGridViewModel
{
    public Guid Id { get; init; }

    [GridColumn(DisplayName = "Username", Order = 1)]
    public string UserName { get; init; } = "";

    [GridColumn(DisplayName = "Email", Order = 2)]
    public string Email { get; init; } = "";

    [GridColumn(DisplayName = "Email Confirmed", Order = 3)]
    public bool EmailConfirmed { get; init; }

    public static UserGridViewModel FromResponse(UserViewModel response) => new()
    {
        Id = response.Id,
        UserName = response.UserName,
        Email = response.Email,
        EmailConfirmed = response.EmailConfirmed
    };
}
