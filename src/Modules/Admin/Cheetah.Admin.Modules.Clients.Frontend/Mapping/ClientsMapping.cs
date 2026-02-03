using Cheetah.Admin.Modules.Clients.Contracts.Response;
using Cheetah.Admin.Modules.Clients.Frontend.Models;
using Cheetah.Mapping.Mapster;
using Mapster;

namespace Cheetah.Admin.Modules.Clients.Frontend.Mapping;

public class ClientsMapping : IMapsterMappingProfile
{
    public void Configure(TypeAdapterConfig config)
    {
        config.NewConfig<ClientViewModel, ClientGridViewModel>()
            .Map(dest => dest.TenantName, src => src.Tenant != null ? src.Tenant.Name : null);

        config.NewConfig<ClientViewModel, ClientFormModel>();
    }
}