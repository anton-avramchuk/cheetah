using Cheetah.Contracts.Responses;

namespace Cheetah.Admin.Modules.Clients.Contracts.Response;

public record TariffViewModel(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    bool IsActive) : ICrmResponse;
