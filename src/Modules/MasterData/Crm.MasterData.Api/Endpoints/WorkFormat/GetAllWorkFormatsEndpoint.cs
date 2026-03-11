using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.MasterData.Application;
using Crm.MasterData.Application.Queries;
using Crm.MasterData.Contracts.Requests;
using Crm.MasterData.Contracts.Response;

namespace Crm.MasterData.Api.Endpoints.WorkFormat;

public class GetAllWorkFormatsEndpoint : QueryGridEndpoint<GetAllWorkFormatsRequest,
    GetAllWorkFormatsQuery, WorkFormatModel, WorkFormatViewModel>
{
    public override string Route => Constants.WorkFormatsRoute;

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("WorkFormats");
    }
}
