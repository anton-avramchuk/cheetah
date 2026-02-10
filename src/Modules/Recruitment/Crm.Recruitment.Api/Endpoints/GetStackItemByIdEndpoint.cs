using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Recruitment.Application;
using Crm.Recruitment.Application.Queries;
using Crm.Recruitment.Contracts.Requests;
using Crm.Recruitment.Contracts.Response;

namespace Crm.Recruitment.Api.Endpoints;

public class GetStackItemByIdEndpoint : QueryOrNotFoundEndpoint<GetStackItemByIdRequest, GetStackItemByIdQuery, StackItemModel,
    StackItemViewModel>
{
    public override string Route => $"{Constants.StackItemRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithName("GetStackItemById");
    }
}
