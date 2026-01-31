namespace Cheetah.Admin.Modules.Clients.Application;

public record TariffModel(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    bool IsActive);
