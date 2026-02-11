using Cheetah.Core.CQRS;

namespace Crm.VacancyTasks.Application.Commands;

public record CreateVacancyTaskCommand(string Name, string? Description) : ICommand<Guid>;