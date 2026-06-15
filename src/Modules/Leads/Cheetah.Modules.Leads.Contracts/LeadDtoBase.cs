using Cheetah.Contracts.Responses;

namespace Cheetah.Modules.Leads.Contracts;

/// <summary>
/// Базовый ViewModel лида (граница API). Абстрактен: наследник объявляет конкретный
/// <c>sealed record LeadDto : LeadDtoBase</c> и при необходимости добавляет свои поля.
/// Статус/источник — ссылки на справочники (<c>StatusId</c>/<c>SourceId</c>); названия отдаются
/// отдельными lookup-эндпоинтами. Контакты — строки (VO живут только в Domain).
/// </summary>
public abstract record LeadDtoBase : ICrmResponse
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = null!;
    public string? Company { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public Guid SourceId { get; init; }
    public Guid StatusId { get; init; }
    public int Score { get; init; }
    public Guid? OwnerId { get; init; }
    public Guid? ConvertedCustomerId { get; init; }
    public Guid? ConvertedDealId { get; init; }
    public string? DisqualifyReason { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
