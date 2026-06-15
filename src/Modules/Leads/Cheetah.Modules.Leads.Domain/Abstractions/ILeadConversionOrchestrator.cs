namespace Cheetah.Modules.Leads.Domain.Abstractions;

/// <summary>
/// Порт конвертации лида в клиента (+ опц. сделку). Главная точка расширения модуля: шаблон не знает,
/// как именно создаются Customer/Deal, — реализацию подключает наследник (прямые Client-вызовы,
/// <c>Cheetah.Saga</c> или публикация события). Так Application остаётся развязанным с Customer/Deals.
/// </summary>
public interface ILeadConversionOrchestrator
{
    ValueTask<ConversionOutcome> ConvertAsync(LeadConversionRequest request, CancellationToken ct = default);
}

/// <summary>Данные для конвертации (снимок полей лида + параметры создаваемой сделки).</summary>
public sealed record LeadConversionRequest(
    Guid LeadId,
    string FullName,
    string? Email,
    string? Phone,
    string? Company,
    bool CreateDeal,
    string? DealTitle,
    decimal? Amount,
    string? Currency,
    Guid? PipelineId);

/// <summary>Итог конвертации: идентификатор созданного клиента и (опц.) сделки.</summary>
public sealed record ConversionOutcome(Guid CustomerId, Guid? DealId);
