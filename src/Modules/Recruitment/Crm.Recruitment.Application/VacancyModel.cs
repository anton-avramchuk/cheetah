namespace Crm.Recruitment.Application;

public record VacancyModel(
    Guid Id,
    string Name,
    string? Description,
    Guid? StateId,
    string? StateName,
    string? StateColor,
    Guid? CustomerId,
    string? CustomerName,
    string? CustomerCode,
    Guid? PositionId,
    string? PositionName,
    Guid? StackItemId,
    string? StackItemName,
    Guid? WorkFormatId,
    string? WorkFormatName,
    int Order,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt);