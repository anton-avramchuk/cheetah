namespace Cheetah.Modules.Customer.Contracts;

/// <summary>
/// Базовый запрос на обновление контактного лица. Абстрактен: наследник объявляет конкретный
/// <c>sealed record UpdateContactRequest : UpdateContactRequestBase</c>.
/// </summary>
public abstract record UpdateContactRequestBase
{
    public string FullName { get; init; } = null!;
    public string? Position { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
}
