using Cheetah.Admin.Modules.Clients.Contracts.Requests;
using Cheetah.Admin.Modules.Clients.Contracts.Response;

namespace Cheetah.Admin.Modules.Clients.Api.Client;

/// <summary>
/// HTTP client for Admin Clients API
/// </summary>
public interface IAdminClientsService
{
    /// <summary>
    /// Gets all clients
    /// </summary>
    ValueTask<IReadOnlyList<ClientViewModel>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets a client by ID
    /// </summary>
    ValueTask<ClientViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Creates a new client
    /// </summary>
    ValueTask<Guid> CreateAsync(CreateClientRequest request, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing client
    /// </summary>
    ValueTask UpdateAsync(Guid id, UpdateClientRequest request, CancellationToken ct = default);
}
