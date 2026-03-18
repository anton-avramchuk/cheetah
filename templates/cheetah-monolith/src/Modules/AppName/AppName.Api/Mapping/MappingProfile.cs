using AppName.Application;
using AppName.Application.Commands;
using AppName.Application.Queries;
using AppName.Contracts.Requests;
using AppName.Contracts.Response;
using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Mapster;
using Mapster;

namespace AppName.Api.Mapping;

[Export(LifetimeType.Singleton, typeof(IMapsterMappingProfile))]
public class MappingProfile : IMapsterMappingProfile
{
    public void Configure(TypeAdapterConfig config)
    {
        config.NewConfig<SampleEntityModel, SampleEntityViewModel>();
        config.NewConfig<GetSampleEntitiesGridRequest, GetSampleEntitiesGridQuery>();
        config.NewConfig<GetSampleEntityByIdRequest, GetSampleEntityByIdQuery>();
        config.NewConfig<CreateSampleEntityRequest, CreateSampleEntityCommand>();
        config.NewConfig<UpdateSampleEntityRequest, UpdateSampleEntityCommand>();
        config.NewConfig<DeleteSampleEntityRequest, DeleteSampleEntityCommand>();
    }
}
