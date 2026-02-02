using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Commands;

public record CreateVacancyCommand(string Name, string? Description) : ICommand<Guid>;