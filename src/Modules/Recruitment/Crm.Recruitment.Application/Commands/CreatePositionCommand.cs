using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Commands;

public record CreatePositionCommand(string Name) : ICommand<Guid>;
