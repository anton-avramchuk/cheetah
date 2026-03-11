using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Mapster;
using Crm.MasterData.Application;
using Crm.MasterData.Application.Commands;
using Crm.MasterData.Application.Queries;
using Crm.MasterData.Contracts.Requests;
using Crm.MasterData.Contracts.Response;
using Mapster;

namespace Crm.MasterData.Api.Mapping;

[Export(LifetimeType.Singleton, typeof(IMapsterMappingProfile))]
public class MappingProfile : IMapsterMappingProfile
{
    public void Configure(TypeAdapterConfig config)
    {
        config.NewConfig<StackItemModel, StackItemViewModel>();
        config.NewConfig<GetAllSampleEntitiesRequest, GetAllSampleEntitiesQuery>();
        config.NewConfig<GetStackItemByIdRequest, GetStackItemByIdQuery>();
        config.NewConfig<CreateStackItemRequest, CreateStackItemCommand>();
        config.NewConfig<UpdateStackItemRequest, UpdateStackItemCommand>();
        config.NewConfig<DeleteStackItemRequest, DeleteStackItemCommand>();
    }
}