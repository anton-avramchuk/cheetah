using Cheetah.Contracts.Responses;
using Cheetah.Modules.Leads.Shared;

namespace Cheetah.Modules.Leads.Contracts;

/// <summary>
/// Базовый запрос на создание лида. Абстрактен: наследник объявляет конкретный
/// <c>sealed record CreateLeadRequest : CreateLeadRequestBase</c> и добавляет свои поля.
/// </summary>
public abstract record CreateLeadRequestBase
{
    public string FullName { get; init; } = null!;
    public LeadSource Source { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Company { get; init; }
    public Guid? OwnerId { get; init; }
}

/// <summary>Базовый запрос на обновление базовых полей лида.</summary>
public abstract record UpdateLeadRequestBase
{
    public string FullName { get; init; } = null!;
    public string? Company { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
}

/// <summary>Базовый запрос на конвертацию лида в клиента (+ опц. сделку).</summary>
public abstract record ConvertLeadRequestBase
{
    public bool CreateDeal { get; init; }
    public string? DealTitle { get; init; }
    public decimal? Amount { get; init; }
    public string? Currency { get; init; }
    public Guid? PipelineId { get; init; }
}

/// <summary>Результат конвертации лида.</summary>
public sealed record ConvertLeadResult(Guid CustomerId, Guid? DealId) : ICrmResponse;
