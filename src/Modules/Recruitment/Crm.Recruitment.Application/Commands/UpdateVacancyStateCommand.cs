using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Commands;

public record UpdateVacancyStateCommand(Guid Id, string Name, int Order, string? Color, bool IsDefault) : ICommand;
