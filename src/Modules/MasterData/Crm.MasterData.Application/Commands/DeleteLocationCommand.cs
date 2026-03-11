using Cheetah.Core.CQRS;

namespace Crm.MasterData.Application.Commands;

public record DeleteLocationCommand(Guid Id) : ICommand;
