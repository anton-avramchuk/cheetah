using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Recruitment.Application.Commands;
using Crm.Recruitment.Contracts.Requests;

namespace Crm.Recruitment.Api.Endpoints;

public class CreateStackItemEndpoint : CreateCommandEndpoint<CreateStackItemRequest, CreateStackItemCommand>
{
    public override string Route => Constants.StackItemRoute;

    public override string GetByIdRouteName => "GetStackItemById";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Stack Items");
    }
}
