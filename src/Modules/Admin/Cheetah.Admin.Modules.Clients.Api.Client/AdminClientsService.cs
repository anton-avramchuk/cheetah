using System.Net;
using System.Net.Http.Json;
using System.Web;
using Cheetah.Admin.Modules.Clients.Contracts.Requests;
using Cheetah.Admin.Modules.Clients.Contracts.Response;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;

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

    public async ValueTask<GridResult<ClientViewModel>> GetAllAsync(
        GetAllClientsRequest? request = null,
        CancellationToken ct = default)
    {
        var url = BuildGridUrl(ClientsPath, request);
        var response = await _httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GridResult<ClientViewModel>>(ct);
        return result ?? new GridResult<ClientViewModel> { Data = [], Total = 0 };
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

    public async ValueTask<GridResult<TariffViewModel>> GetAllTariffsAsync(
        GetAllTariffsRequest? request = null,
        CancellationToken ct = default)
    {
        var url = BuildGridUrl(TariffsPath, request);
        var response = await _httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GridResult<TariffViewModel>>(ct);
        return result ?? new GridResult<TariffViewModel> { Data = [], Total = 0 };
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

    #region Private Methods

    private static string BuildGridUrl(string basePath, GridRequest? request)
    {
        if (request is null)
            return basePath;

        var queryParams = HttpUtility.ParseQueryString(string.Empty);

        queryParams["page"] = request.Page.ToString();
        queryParams["pageSize"] = request.PageSize.ToString();

        for (var i = 0; i < request.Sort.Count; i++)
        {
            var sort = request.Sort[i];
            if (!string.IsNullOrEmpty(sort.Field))
            {
                queryParams[$"sort[{i}][field]"] = sort.Field;
                queryParams[$"sort[{i}][dir]"] = sort.Dir ?? "asc";
            }
        }

        // Note: Filter serialization for complex nested filters is simplified here
        // For production, consider using a proper serialization strategy
        if (request.Filter is not null && !string.IsNullOrEmpty(request.Filter.Field))
        {
            queryParams["filter[field]"] = request.Filter.Field;
            queryParams["filter[operator]"] = request.Filter.Operator ?? "eq";
            queryParams["filter[value]"] = request.Filter.Value?.ToString() ?? string.Empty;
        }

        var queryString = queryParams.ToString();
        return string.IsNullOrEmpty(queryString) ? basePath : $"{basePath}?{queryString}";
    }

    #endregion
}
