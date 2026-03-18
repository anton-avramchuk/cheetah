using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.MasterData.Application;
using Crm.MasterData.Application.Queries;
using Crm.MasterData.Contracts.Requests;
using Crm.MasterData.Contracts.Response;

namespace Crm.MasterData.Api.Endpoints.Position;

public class GetPositionsGridEndpoint : QueryGridEndpoint<GetPositionsGridRequest,
    GetPositionsGridQuery, PositionModel, PositionViewModel>
{
    public override string Route => Constants.PositionsRoute;

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Positions");
    }
}
