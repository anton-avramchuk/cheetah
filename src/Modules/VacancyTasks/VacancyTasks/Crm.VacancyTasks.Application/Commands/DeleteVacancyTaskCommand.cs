using Cheetah.Core.CQRS;

namespace Crm.VacancyTasks.Application.Commands;

public record DeleteVacancyTaskCommand(Guid Id) : ICommand;