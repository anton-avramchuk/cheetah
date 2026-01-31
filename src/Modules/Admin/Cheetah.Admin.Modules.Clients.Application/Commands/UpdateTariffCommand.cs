using Cheetah.Core.CQRS;

namespace Cheetah.Admin.Modules.Clients.Application.Commands;

public record UpdateTariffCommand(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    bool IsActive) : ICommand;
