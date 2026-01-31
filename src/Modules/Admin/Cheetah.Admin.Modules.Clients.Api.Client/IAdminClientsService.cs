using Cheetah.Admin.Modules.Clients.Contracts.Requests;
using Cheetah.Admin.Modules.Clients.Contracts.Response;

namespace Cheetah.Admin.Modules.Clients.Api.Client;

/// <summary>
/// HTTP client for Admin Clients API
/// </summary>
public interface IAdminClientsService
{
    #region Clients

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

    #endregion

    #region Tariffs

    /// <summary>
    /// Gets all tariffs
    /// </summary>
    ValueTask<IReadOnlyList<TariffViewModel>> GetAllTariffsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets a tariff by ID
    /// </summary>
    ValueTask<TariffViewModel?> GetTariffByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Creates a new tariff
    /// </summary>
    ValueTask<Guid> CreateTariffAsync(CreateTariffRequest request, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing tariff
    /// </summary>
    ValueTask UpdateTariffAsync(Guid id, UpdateTariffRequest request, CancellationToken ct = default);

    /// <summary>
    /// Deletes a tariff
    /// </summary>
    ValueTask DeleteTariffAsync(Guid id, CancellationToken ct = default);

    #endregion
}
