using Cheetah.Core.CQRS;

namespace Cheetah.Admin.Modules.Clients.Application.Commands;

public record CreateTariffCommand(
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    bool IsActive) : ICommand<Guid>;
