using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Mapster;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Application.Queries;
using Cheetah.Modules.Identity.Contracts.Requests;
using Cheetah.Modules.Identity.Contracts.Response;
using Cheetah.Modules.Identity.Domain;
using Mapster;

namespace Cheetah.Modules.Identity.Api.Mapping;

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
        config.NewConfig<CrmIdentityRole, RoleModel>();
        config.NewConfig<RoleModel, RoleViewModel>();
        config.NewConfig<GetAllRolesRequest, GetAllRolesQuery>();
        config.NewConfig<GetRoleByIdRequest, GetRoleByIdQuery>();
        config.NewConfig<CreateRoleRequest, CreateRoleCommand>();
        config.NewConfig<UpdateRoleRequest, UpdateRoleCommand>();
        config.NewConfig<DeleteRoleRequest, DeleteRoleCommand>();

        // Users
        config.NewConfig<CrmIdentityUser, UserModel>();
        config.NewConfig<CrmIdentityUser, UserDetailModel>()
            .Map(dest => dest.RoleIds, src => src.Roles.Select(r => r.RoleId).ToList());
        config.NewConfig<UserModel, UserGridViewModel>();
        config.NewConfig<UserDetailModel, UserDetailViewModel>();
        config.NewConfig<GetAllUsersRequest, GetAllUsersQuery>();
        config.NewConfig<GetUserByIdRequest, GetUserByIdQuery>();
        config.NewConfig<CreateUserRequest, CreateUserCommand>();
        config.NewConfig<UpdateUserRequest, UpdateUserCommand>();
        config.NewConfig<DeleteUserRequest, DeleteUserCommand>();
    }
}
