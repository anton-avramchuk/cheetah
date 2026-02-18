using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Commands;

public record CreateVacancyStateCommand(string Name, int Order, string? Color, bool IsDefault) : ICommand<Guid>;
