using Cheetah.Core.CQRS;

namespace Crm.VacancyTasks.Application.Commands;

public record DeleteTaskStateCommand(Guid Id) : ICommand;
