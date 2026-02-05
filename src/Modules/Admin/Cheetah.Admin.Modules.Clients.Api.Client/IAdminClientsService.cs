using Cheetah.Admin.Modules.Clients.Contracts.Requests;
using Cheetah.Admin.Modules.Clients.Contracts.Response;
using Cheetah.Contracts.Responses;

namespace Cheetah.Admin.Modules.Clients.Api.Client;

/// <summary>
/// HTTP client for Admin Clients API
/// </summary>
public interface IAdminClientsService
{
    #region Clients

    /// <summary>
    /// Gets clients with pagination, sorting, and filtering
    /// </summary>
    ValueTask<GridResult<ClientViewModel>> GetAllAsync(
        GetAllClientsRequest? request = null,
        CancellationToken ct = default);

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
    /// Gets tariffs with pagination, sorting, and filtering
    /// </summary>
    ValueTask<GridResult<TariffViewModel>> GetAllTariffsAsync(
        GetAllTariffsRequest? request = null,
        CancellationToken ct = default);

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
