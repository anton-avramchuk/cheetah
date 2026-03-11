using Cheetah.Core.CQRS;

namespace Crm.MasterData.Application.Commands;

public record DeleteWorkFormatCommand(Guid Id) : ICommand;
