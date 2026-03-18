using AppName.Identity.Domain;
using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Mapster;
using Cheetah.Modules.Identity.Application.Models;
using Mapster;

namespace AppName.Identity.Api.Mapping;

[Export(LifetimeType.Singleton, typeof(IMapsterMappingProfile))]
public class MappingProfile : IMapsterMappingProfile
{
    public void Configure(TypeAdapterConfig config)
    {
        config.NewConfig<AppNameIdentityRole, RoleModel>();

        config.NewConfig<AppNameIdentityUser, UserModel>();
        config.NewConfig<AppNameIdentityUser, UserDetailModel>()
            .Map(dest => dest.RoleIds, src => src.Roles.Select(r => r.RoleId).ToList());
    }
}
