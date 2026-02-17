namespace Crm.Recruitment.Application;

public record VacancyModel(
    Guid Id,
    string Name,
    string? Description,
    Guid? StateId,
    string? StateName,
    Guid? CustomerId,
    string? CustomerName,
    Guid? PositionId,
    string? PositionName,
    Guid? StackItemId,
    string? StackItemName,
    Guid? WorkFormatId,
    string? WorkFormatName,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt);