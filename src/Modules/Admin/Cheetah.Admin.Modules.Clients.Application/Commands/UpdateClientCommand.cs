using Cheetah.Core.CQRS;

namespace Cheetah.Admin.Modules.Clients.Application.Commands;

public record UpdateClientCommand(Guid Id, string Name, string? Description) : ICommand;
