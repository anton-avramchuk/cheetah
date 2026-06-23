using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Mapster;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Application.Queries;
using Cheetah.Modules.Identity.Contracts.Requests;
using Cheetah.Modules.Identity.Contracts.Response;
using Mapster;

namespace Cheetah.Modules.Identity.Mapster.Mapping;

[Export(LifetimeType.Singleton, typeof(IMapsterMappingProfile))]
public class MappingProfile : IMapsterMappingProfile
{
    public void Configure(TypeAdapterConfig config)
    {
        // Auth
        config.NewConfig<LoginRequest, LoginCommand>();
        config.NewConfig<TokenResult, TokenViewModel>()
            .Map(dest => dest.AccessToken, src => src.Token)
            .Map(dest => dest.TokenType, _ => "Bearer")
            .Map(dest => dest.ExpiresIn, src => src.ExpiresInSeconds);

        // Roles
        config.NewConfig<RoleModel, RoleViewModel>();
        config.NewConfig(typeof(GetRolesGridRequest), typeof(GetRolesGridQuery<>));
        config.NewConfig<GetRoleByIdRequest, GetRoleByIdQuery>();
        config.NewConfig<CreateRoleRequest, CreateRoleCommand>();
        config.NewConfig<UpdateRoleRequest, UpdateRoleCommand>();
        config.NewConfig<DeleteRoleRequest, DeleteRoleCommand>();

        // Users
        config.NewConfig<UserModel, UserGridViewModel>();
        config.NewConfig<UserDetailModel, UserDetailViewModel>();
        config.NewConfig(typeof(GetUsersGridRequest), typeof(GetUsersGridQuery<>));
        config.NewConfig<GetUserByIdRequest, GetUserByIdQuery>();
        config.NewConfig<CreateUserRequest, CreateUserCommand>();
        config.NewConfig<UpdateUserRequest, UpdateUserCommand>();
        config.NewConfig<DeleteUserRequest, DeleteUserCommand>();
    }
}
