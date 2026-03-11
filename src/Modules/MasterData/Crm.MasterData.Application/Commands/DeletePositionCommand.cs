using Cheetah.Core.CQRS;

namespace Crm.MasterData.Application.Commands;

public record DeletePositionCommand(Guid Id) : ICommand;
