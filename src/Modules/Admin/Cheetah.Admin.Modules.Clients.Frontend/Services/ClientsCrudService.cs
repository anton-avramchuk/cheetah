using Cheetah.Admin.Modules.Clients.Api.Client;
using Cheetah.Admin.Modules.Clients.Contracts.Requests;
using Cheetah.Admin.Modules.Clients.Frontend.Models;
using Cheetah.Blazor.Components.Crud;
using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Core;

namespace Cheetah.Admin.Modules.Clients.Frontend.Services;

/// <summary>
/// CRUD service adapter for clients.
/// </summary>
[Export(LifetimeType.Scoped, typeof(ICrudService<ClientGridViewModel, ClientFormModel, ClientFormModel>))]
public sealed class ClientsCrudService : ICrudService<ClientGridViewModel, ClientFormModel, ClientFormModel>
{
    private readonly IAdminClientsService _clientsService;
    private readonly IObjectMapper _objectMapper;

    public ClientsCrudService(IAdminClientsService clientsService, IObjectMapper objectMapper)
    {
        _clientsService = clientsService;
        _objectMapper = objectMapper;
    }

    public async Task<IReadOnlyList<ClientGridViewModel>> GetAllAsync(CancellationToken ct = default)
    {
        var result = await _clientsService.GetAllAsync(null, ct);

        return _objectMapper.Map<IReadOnlyList<ClientGridViewModel>>(result.Data);
    }

    public async Task<ClientFormModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var client = await _clientsService.GetByIdAsync(id, ct);
        if (client == null)
            return null;
        
        return  _objectMapper.Map<ClientFormModel>(client);
    }

    public async Task<Guid> CreateAsync(ClientFormModel model, CancellationToken ct = default)
    {
        var description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description;
        var request = new CreateClientRequest(model.Name, description);
        return await _clientsService.CreateAsync(request, ct);
    }

    public async Task UpdateAsync(Guid id, ClientFormModel model, CancellationToken ct = default)
    {
        var description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description;
        var request = new UpdateClientRequest(id, model.Name, description);
        await _clientsService.UpdateAsync(id, request, ct);
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // TODO: Implement delete when API supports it
        throw new NotSupportedException("Delete is not supported yet");
    }
}