using Cheetah.Contracts.Responses;

namespace Cheetah.Modules.Teams.Contracts;

/// <summary>
/// Базовый ViewModel команды со составом (граница API). Абстрактен: наследник объявляет конкретный
/// <c>sealed record TeamDto : TeamDtoBase</c> и при необходимости добавляет свои поля (отдел, цвет).
/// Это и есть точка расширяемости ViewModel.
/// </summary>
public abstract record TeamDtoBase : ICrmResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public bool IsActive { get; init; }
    public IReadOnlyList<TeamMembershipDto> Members { get; init; } = Array.Empty<TeamMembershipDto>();
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

/// <summary>
/// Базовый «лёгкий» ViewModel команды для грида (без состава). Абстрактен: наследник объявляет
/// <c>sealed record TeamGridViewModel : TeamGridViewModelBase</c> и добавляет свои колонки.
/// </summary>
public abstract record TeamGridViewModelBase : ICrmResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public bool IsActive { get; init; }
}

/// <summary>Членство в команде: участник + его роль.</summary>
public sealed record TeamMembershipDto
{
    public Guid MemberId { get; init; }
    public Guid RoleId { get; init; }
}
