using System.Net;
using System.Net.Http.Json;
using Cheetah.Admin.Modules.Clients.Contracts.Requests;
using Cheetah.Admin.Modules.Clients.Contracts.Response;

namespace Cheetah.Admin.Modules.Clients.Api.Client;

internal record CreateEntityResponse(Guid Id);

public class AdminClientsService : IAdminClientsService
{
    private const string ClientsPath = "api/clients";
    private const string TariffsPath = "api/tariffs";
    private readonly HttpClient _httpClient;

    public AdminClientsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    #region Clients

    public async ValueTask<IReadOnlyList<ClientViewModel>> GetAllAsync(CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync(ClientsPath, ct);
        response.EnsureSuccessStatusCode();

        var clients = await response.Content.ReadFromJsonAsync<List<ClientViewModel>>(ct);
        return clients ?? [];
    }

    public async ValueTask<ClientViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"{ClientsPath}/{id}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ClientViewModel>(ct);
    }

    public async ValueTask<Guid> CreateAsync(CreateClientRequest request, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync(ClientsPath, request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CreateEntityResponse>(ct);
        return result?.Id ?? throw new InvalidOperationException("Failed to parse response");
    }

    public async ValueTask UpdateAsync(Guid id, UpdateClientRequest request, CancellationToken ct = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"{ClientsPath}/{id}", request, ct);
        response.EnsureSuccessStatusCode();
    }

    #endregion

    #region Tariffs

    public async ValueTask<IReadOnlyList<TariffViewModel>> GetAllTariffsAsync(CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync(TariffsPath, ct);
        response.EnsureSuccessStatusCode();

        var tariffs = await response.Content.ReadFromJsonAsync<List<TariffViewModel>>(ct);
        return tariffs ?? [];
    }

    public async ValueTask<TariffViewModel?> GetTariffByIdAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"{TariffsPath}/{id}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TariffViewModel>(ct);
    }

    public async ValueTask<Guid> CreateTariffAsync(CreateTariffRequest request, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync(TariffsPath, request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CreateEntityResponse>(ct);
        return result?.Id ?? throw new InvalidOperationException("Failed to parse response");
    }

    public async ValueTask UpdateTariffAsync(Guid id, UpdateTariffRequest request, CancellationToken ct = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"{TariffsPath}/{id}", request, ct);
        response.EnsureSuccessStatusCode();
    }

    public async ValueTask DeleteTariffAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _httpClient.DeleteAsync($"{TariffsPath}/{id}", ct);
        response.EnsureSuccessStatusCode();
    }

    #endregion
}
