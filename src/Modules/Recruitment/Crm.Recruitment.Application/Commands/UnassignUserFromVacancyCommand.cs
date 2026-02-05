using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Commands;

public record UnassignUserFromVacancyCommand(Guid AssignmentId) : ICommand;
