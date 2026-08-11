namespace Cheetah.Modules.Customer.Contracts;

/// <summary>
/// Базовый запрос на обновление контактного лица. Абстрактен: наследник объявляет конкретный
/// <c>sealed record UpdateContactRequest : UpdateContactRequestBase</c>.
/// </summary>
public abstract record UpdateContactRequestBase
{
    public string FullName { get; init; } = null!;
    public Guid? PositionId { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }

    /// <summary>Ник Telegram без «@» — ссылку «написать» строит интерфейс.</summary>
    public string? Telegram { get; init; }

    /// <summary>Номер WhatsApp: по форме телефон, но способ связи другой.</summary>
    public string? WhatsApp { get; init; }
}
