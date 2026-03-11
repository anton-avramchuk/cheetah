using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.MasterData.Application;
using Crm.MasterData.Application.Queries;
using Crm.MasterData.Contracts.Requests;
using Crm.MasterData.Contracts.Response;

namespace Crm.MasterData.Api.Endpoints.WorkFormat;

public class GetWorkFormatByIdEndpoint : QueryOrNotFoundEndpoint<GetWorkFormatByIdRequest, GetWorkFormatByIdQuery,
    WorkFormatModel, WorkFormatViewModel>
{
    public override string Route => $"{Constants.WorkFormatsRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithName("GetWorkFormatById");
        config.WithTags("WorkFormats");
    }
}
