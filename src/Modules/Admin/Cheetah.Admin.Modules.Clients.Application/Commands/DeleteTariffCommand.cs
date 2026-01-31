using Cheetah.Core.CQRS;

namespace Cheetah.Admin.Modules.Clients.Application.Commands;

public record DeleteTariffCommand(Guid Id) : ICommand;
