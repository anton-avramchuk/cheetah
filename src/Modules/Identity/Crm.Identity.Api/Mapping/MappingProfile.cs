using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Mapster;
using Crm.Identity.Application;
using Crm.Identity.Application.Commands;
using Crm.Identity.Application.Queries;
using Crm.Identity.Contracts.Requests;
using Crm.Identity.Contracts.Response;
using Mapster;

namespace Crm.Identity.Api.Mapping;

[Export(LifetimeType.Singleton, typeof(IMapsterMappingProfile))]
public class MappingProfile : IMapsterMappingProfile
{
    public void Configure(TypeAdapterConfig config)
    {
        config.NewConfig<UserIdentityModel, UserIdentityViewModel>();
        config.NewConfig<GetAllSampleEntitiesRequest, GetAllSampleEntitiesQuery>();
        config.NewConfig<GetUserIdentityByIdRequest, GetUserIdentityByIdQuery>();
        config.NewConfig<CreateUserIdentityRequest, CreateUserIdentityCommand>();
        config.NewConfig<UpdateUserIdentityRequest, UpdateUserIdentityCommand>();
        config.NewConfig<DeleteUserIdentityRequest, DeleteUserIdentityCommand>();
    }
}