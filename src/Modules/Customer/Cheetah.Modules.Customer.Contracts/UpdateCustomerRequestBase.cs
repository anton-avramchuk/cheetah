namespace Cheetah.Modules.Customer.Contracts;

/// <summary>
/// Базовый запрос на обновление клиента. Абстрактен: наследник объявляет конкретный
/// <c>sealed record UpdateCustomerRequest : UpdateCustomerRequestBase</c>.
/// </summary>
public abstract record UpdateCustomerRequestBase
{
    public string DisplayName { get; init; } = null!;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public Guid? OwnerId { get; init; }
}
