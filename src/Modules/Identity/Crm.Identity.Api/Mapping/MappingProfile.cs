using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Mapster;
using Cheetah.Modules.Identity.Application.Models;
using Crm.Identity.Domain;
using Mapster;

namespace Crm.Identity.Api.Mapping;

[Export(LifetimeType.Singleton, typeof(IMapsterMappingProfile))]
public class MappingProfile : IMapsterMappingProfile
{
    public void Configure(TypeAdapterConfig config)
    {
        config.NewConfig<CrmIdentityRole, RoleModel>();

        config.NewConfig<CrmIdentityUser, UserModel>();
        config.NewConfig<CrmIdentityUser, UserDetailModel>()
            .Map(dest => dest.RoleIds, src => src.Roles.Select(r => r.RoleId).ToList());
    }
}
