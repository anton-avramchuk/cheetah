using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Recruitment.Application.Commands;
using Crm.Recruitment.Contracts.Requests;

namespace Crm.Recruitment.Api.Endpoints;

public class CreateWorkFormatEndpoint : CreateCommandEndpoint<CreateWorkFormatRequest, CreateWorkFormatCommand>
{
    public override string Route => Constants.WorkFormatRoute;

    public override string GetByIdRouteName => "GetWorkFormatById";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Work Formats");
    }
}
