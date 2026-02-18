using Cheetah.Contracts.Responses;

namespace Crm.Recruitment.Contracts.Response;

public record VacancyViewModel(
    Guid Id,
    string Name,
    string? Description,
    Guid? StateId = null,
    string? StateName = null,
    string? StateColor = null,
    Guid? CustomerId = null,
    string? CustomerName = null,
    string? CustomerCode = null,
    Guid? PositionId = null,
    string? PositionName = null,
    Guid? StackItemId = null,
    string? StackItemName = null,
    Guid? WorkFormatId = null,
    string? WorkFormatName = null,
    int Order = 0,
    DateTimeOffset? CreatedAt = null,
    DateTimeOffset? UpdatedAt = null) : ICrmResponse;