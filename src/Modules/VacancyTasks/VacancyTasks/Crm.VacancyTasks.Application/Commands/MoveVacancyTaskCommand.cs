using Cheetah.Core.CQRS;

namespace Crm.VacancyTasks.Application.Commands;

public record MoveVacancyTaskCommand(Guid Id, Guid StateId, int Order) : ICommand;
