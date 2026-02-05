using Cheetah.Admin.Modules.Clients.Application;
using Cheetah.Admin.Modules.Clients.Application.Commands;
using Cheetah.Admin.Modules.Clients.Application.Queries;
using Cheetah.Admin.Modules.Clients.Contracts.Requests;
using Cheetah.Admin.Modules.Clients.Contracts.Response;
using Cheetah.Admin.Modules.Clients.Domain;
using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Mapster;
using Mapster;

namespace Cheetah.Admin.Modules.Clients.Api.Mapping;

[Export(LifetimeType.Singleton, typeof(IMapsterMappingProfile))]
public class MappingProfile : IMapsterMappingProfile
{
    public void Configure(TypeAdapterConfig config)
    {
        // Client entity -> Model (for ProjectTo)
        config.NewConfig<Client, ClientModel>()
            .Map(dest => dest.Tenant, src => src.TenantId.HasValue
                ? new TenantModel(src.TenantId.Value, "Default")
                : null);

        // Tariff entity -> Model (for ProjectTo)
        config.NewConfig<Tariff, TariffModel>();

        // Client mappings
        config.NewConfig<TenantModel, TenantViewModel>();
        config.NewConfig<ClientModel, ClientViewModel>();
        config.NewConfig<GetAllClientsRequest, GetAllClientsQuery>();
        config.NewConfig<GetClientByIdRequest, GetClientByIdQuery>();
        config.NewConfig<CreateClientRequest, CreateClientCommand>();
        config.NewConfig<UpdateClientRequest, UpdateClientCommand>();

        // Tariff mappings
        config.NewConfig<TariffModel, TariffViewModel>();
        config.NewConfig<GetAllTariffsRequest, GetAllTariffsQuery>();
        config.NewConfig<GetTariffByIdRequest, GetTariffByIdQuery>();
        config.NewConfig<CreateTariffRequest, CreateTariffCommand>();
        config.NewConfig<UpdateTariffRequest, UpdateTariffCommand>();
        config.NewConfig<DeleteTariffRequest, DeleteTariffCommand>();
    }
}
