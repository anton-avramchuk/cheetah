namespace Cheetah.Modules.Customer.Contracts;

/// <summary>
/// Базовый запрос на добавление контактного лица. Абстрактен: наследник объявляет конкретный
/// <c>sealed record CreateContactRequest : CreateContactRequestBase</c>.
/// <c>CustomerId</c> здесь нет — он берётся из маршрута (<c>api/customers/{customerId}/contacts</c>).
/// </summary>
public abstract record CreateContactRequestBase
{
    public string FullName { get; init; } = null!;
    public string? Position { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
}
