using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Recruitment.Application.Commands;
using Crm.Recruitment.Contracts.Requests;

namespace Crm.Recruitment.Api.Endpoints;

public class CreatePositionEndpoint : CreateCommandEndpoint<CreatePositionRequest, CreatePositionCommand>
{
    public override string Route => Constants.PositionRoute;

    public override string GetByIdRouteName => "GetPositionById";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Positions");
    }
}
