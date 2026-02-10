using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Recruitment.Application;
using Crm.Recruitment.Application.Queries;
using Crm.Recruitment.Contracts.Requests;
using Crm.Recruitment.Contracts.Response;

namespace Crm.Recruitment.Api.Endpoints;

public class GetAllPositionsEndpoint : QueryCollectionEndpoint<GetAllPositionsRequest,
    GetAllPositionsQuery, PositionModel, PositionViewModel>
{
    public override string Route => Constants.PositionRoute;

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Positions");
    }
}
