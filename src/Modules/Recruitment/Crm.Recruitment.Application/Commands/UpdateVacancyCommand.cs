using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Commands;

public record UpdateVacancyCommand(
    Guid Id,
    string Name,
    string? Description,
    Guid? StateId = null,
    Guid? CustomerId = null,
    Guid? PositionId = null,
    Guid? StackItemId = null,
    Guid? WorkFormatId = null) : ICommand;