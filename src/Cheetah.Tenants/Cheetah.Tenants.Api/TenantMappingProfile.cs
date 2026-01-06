using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Mapster;
using Cheetah.Tenants.Application.Commands;
using Cheetah.Tenants.Domain.Entities;
using Cheetah.Tenants.Contracts.Requests;
using Cheetah.Tenants.Contracts.ViewModels;
using Mapster;

namespace Cheetah.Tenants.Api;

[Export(LifetimeType.Singleton, typeof(IMapsterMappingProfile))]
public class TenantMappingProfile : IMapsterMappingProfile
{
    public void Configure(TypeAdapterConfig config)
    {
        // Request to Command mapping
        config.NewConfig<CreateTenantRequest, CreateTenantCommand>();

        // Domain Entity to ViewModel mapping
        config.NewConfig<Tenant, TenantViewModel>()
            .Map(dest => dest.ConnectionStrings,
                 src => src.ConnectionStrings);

        config.NewConfig<TenantConnectionString, TenantConnectionStringViewModel>();
    }
}
