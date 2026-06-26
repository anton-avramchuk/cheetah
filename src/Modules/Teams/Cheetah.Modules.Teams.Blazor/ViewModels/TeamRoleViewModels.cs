using Cheetah.AspNetCore.Blazor.Grid;

namespace Cheetah.Modules.Teams.Blazor.ViewModels;

/// <summary>ViewModel роли команды для грида.</summary>
public sealed class TeamRoleGridViewModel : IHasId
{
    public Guid Id { get; set; }

    [GridColumn("Название", order: 0)]
    public string Name { get; set; } = string.Empty;
}

/// <summary>ViewModel роли команды для формы редактирования.</summary>
public sealed class TeamRoleDetailsViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>ViewModel роли команды для формы создания.</summary>
public sealed class TeamRoleCreateViewModel
{
    public string Name { get; set; } = string.Empty;
}
