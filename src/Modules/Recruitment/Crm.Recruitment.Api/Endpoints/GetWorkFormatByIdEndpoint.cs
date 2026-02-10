using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Recruitment.Application;
using Crm.Recruitment.Application.Queries;
using Crm.Recruitment.Contracts.Requests;
using Crm.Recruitment.Contracts.Response;

namespace Crm.Recruitment.Api.Endpoints;

public class GetWorkFormatByIdEndpoint : QueryOrNotFoundEndpoint<GetWorkFormatByIdRequest, GetWorkFormatByIdQuery, WorkFormatModel,
    WorkFormatViewModel>
{
    public override string Route => $"{Constants.WorkFormatRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithName("GetWorkFormatById").WithTags("Work Formats");
    }
}
