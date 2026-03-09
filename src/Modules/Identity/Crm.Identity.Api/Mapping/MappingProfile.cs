using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Mapster;
using Crm.Identity.Application;
using Crm.Identity.Application.Commands;
using Crm.Identity.Application.Queries;
using Crm.Identity.Contracts.Requests;
using Crm.Identity.Contracts.Response;
using Crm.Identity.Domain;
using Mapster;

namespace Crm.Identity.Api.Mapping;

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
        config.NewConfig<CrmRole, RoleModel>();
        config.NewConfig<RoleModel, RoleViewModel>();
        config.NewConfig<GetAllRolesRequest, GetAllRolesQuery>();
        config.NewConfig<GetRoleByIdRequest, GetRoleByIdQuery>();
        config.NewConfig<CreateRoleRequest, CreateRoleCommand>();
        config.NewConfig<UpdateRoleRequest, UpdateRoleCommand>();
        config.NewConfig<DeleteRoleRequest, DeleteRoleCommand>();

        // Users
        config.NewConfig<CrmUser, UserModel>();
        config.NewConfig<CrmUser, UserDetailModel>()
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
