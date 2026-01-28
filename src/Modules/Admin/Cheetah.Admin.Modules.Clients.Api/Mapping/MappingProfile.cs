using Cheetah.Admin.Modules.Clients.Application.Commands;
using Cheetah.Admin.Modules.Clients.Application.Queries;
using Cheetah.Admin.Modules.Clients.Contracts.Requests;
using Cheetah.Admin.Modules.Clients.Contracts.Response;
using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Mapster;
using Mapster;

namespace Cheetah.Admin.Modules.Clients.Api.Mapping;

[Export(LifetimeType.Singleton, typeof(IMapsterMappingProfile))]
public class MappingProfile : IMapsterMappingProfile
{
    public void Configure(TypeAdapterConfig config)
    {
        config.NewConfig<TenantModel, TenantViewModel>();
        config.NewConfig<ClientModel, ClientViewModel>();
        config.NewConfig<GetAllClientsRequest, GetAllClientsQuery>();
        config.NewConfig<GetClientByIdRequest, GetClientByIdQuery>();
        config.NewConfig<CreateClientRequest, CreateClientCommand>();
        config.NewConfig<UpdateClientRequest, UpdateClientCommand>();
    }
}