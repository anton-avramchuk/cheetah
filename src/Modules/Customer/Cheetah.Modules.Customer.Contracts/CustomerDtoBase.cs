using Cheetah.Modules.Customer.Shared;

namespace Cheetah.Modules.Customer.Contracts;

/// <summary>
/// Базовый ViewModel клиента (граница API). Абстрактен: наследник объявляет конкретный
/// <c>sealed record CustomerDto : CustomerDtoBase</c> и при необходимости добавляет свои поля.
/// Контакты — строки (VO живут только в Domain).
/// </summary>
public abstract record CustomerDtoBase
{
    public Guid Id { get; init; }
    public string DisplayName { get; init; } = null!;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public CustomerStatus Status { get; init; }
    public Guid? OwnerId { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
}
