using Cheetah.Core.CQRS;

namespace Crm.MasterData.Application.Commands;

public record DeleteStackItemCommand(Guid Id) : ICommand;