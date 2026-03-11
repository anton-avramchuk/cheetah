using Cheetah.Core.CQRS;

namespace Crm.MasterData.Application.Commands;

public record UpdateWorkFormatCommand(Guid Id, string Name) : ICommand;
