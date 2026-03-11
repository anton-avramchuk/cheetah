using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.MasterData.Application;
using Crm.MasterData.Application.Queries;
using Crm.MasterData.Contracts.Requests;
using Crm.MasterData.Contracts.Response;

namespace Crm.MasterData.Api.Endpoints.StackItem;

public class GetAllSampleEntitiesEndpoint : QueryGridEndpoint<GetAllSampleEntitiesRequest,
    GetAllSampleEntitiesQuery, StackItemModel, StackItemViewModel>
{
    public override string Route => Constants.DefaultRoute;

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("SampleEntities");
    }
}