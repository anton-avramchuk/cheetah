namespace Cheetah.Modules.Customer.Contracts;

/// <summary>
/// Базовый запрос на создание клиента. Абстрактен: наследник объявляет конкретный
/// <c>sealed record CreateCustomerRequest : CreateCustomerRequestBase</c> и добавляет свои поля.
/// </summary>
public abstract record CreateCustomerRequestBase
{
    public string DisplayName { get; init; } = null!;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public Guid? OwnerId { get; init; }
}
