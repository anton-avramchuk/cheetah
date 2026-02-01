using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Mapster;
using __Prefix__.ModuleName.Application;
using __Prefix__.ModuleName.Application.Commands;
using __Prefix__.ModuleName.Application.Queries;
using __Prefix__.ModuleName.Contracts.Requests;
using __Prefix__.ModuleName.Contracts.Response;
using Mapster;

namespace __Prefix__.ModuleName.Api.Mapping;

[Export(LifetimeType.Singleton, typeof(IMapsterMappingProfile))]
public class MappingProfile : IMapsterMappingProfile
{
    public void Configure(TypeAdapterConfig config)
    {
        config.NewConfig<SampleEntityModel, SampleEntityViewModel>();
        config.NewConfig<GetAllSampleEntitiesRequest, GetAllSampleEntitiesQuery>();
        config.NewConfig<GetSampleEntityByIdRequest, GetSampleEntityByIdQuery>();
        config.NewConfig<CreateSampleEntityRequest, CreateSampleEntityCommand>();
        config.NewConfig<UpdateSampleEntityRequest, UpdateSampleEntityCommand>();
        config.NewConfig<DeleteSampleEntityRequest, DeleteSampleEntityCommand>();
    }
}
