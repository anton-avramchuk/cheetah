using Cheetah.Identity.Application.Commands;
using Cheetah.Identity.Shared.Requests;
using Cheetah.Mapping.Mapster;
using Mapster;

namespace Cheetah.Identity.Api.MappingProfiles;

public class IdentityMappingProfile : IMapsterMappingProfile
{
    public void Configure(TypeAdapterConfig config)
    {
        // RegisterUserRequest -> RegisterUserCommand
        config.NewConfig<RegisterUserRequest, RegisterUserCommand>()
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.Password, src => src.Password)
            .Map(dest => dest.FirstName, src => src.FirstName)
            .Map(dest => dest.LastName, src => src.LastName);

        // ChangePasswordRequest -> ChangePasswordCommand
        // Note: UserId will be set from route parameter in API endpoint
        config.NewConfig<ChangePasswordRequest, ChangePasswordCommand>()
            .Map(dest => dest.CurrentPassword, src => src.CurrentPassword)
            .Map(dest => dest.NewPassword, src => src.NewPassword)
            .Ignore(dest => dest.UserId);

        // CreateRoleRequest -> CreateRoleCommand
        config.NewConfig<CreateRoleRequest, CreateRoleCommand>()
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description);
    }
}
