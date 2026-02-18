using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Commands;

public record MoveVacancyCommand(Guid Id, Guid StateId, int Order) : ICommand;
