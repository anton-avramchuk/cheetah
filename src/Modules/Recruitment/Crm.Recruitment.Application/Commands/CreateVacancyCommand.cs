using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Commands;

public record CreateVacancyCommand(
    string Name,
    string? Description,
    Guid? StateId = null,
    Guid? CustomerId = null,
    Guid? PositionId = null,
    Guid? StackItemId = null,
    Guid? WorkFormatId = null) : ICommand<Guid>;