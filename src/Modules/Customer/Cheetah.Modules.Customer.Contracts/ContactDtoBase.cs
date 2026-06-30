namespace Cheetah.Modules.Customer.Contracts;

/// <summary>
/// Базовый ViewModel контактного лица клиента (граница API). Абстрактен: наследник
/// объявляет конкретный <c>sealed record ContactDto : ContactDtoBase</c> и при необходимости
/// добавляет свои поля. Контакты — строки (VO живут только в Domain).
/// </summary>
public abstract record ContactDtoBase
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public string FullName { get; init; } = null!;
    public Guid? PositionId { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
}
