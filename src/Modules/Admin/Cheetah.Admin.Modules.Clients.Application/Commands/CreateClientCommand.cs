using Cheetah.Core.CQRS;

namespace Cheetah.Admin.Modules.Clients.Application.Commands;

public record CreateClientCommand(string Name, string? Description) : ICommand<Guid>;
