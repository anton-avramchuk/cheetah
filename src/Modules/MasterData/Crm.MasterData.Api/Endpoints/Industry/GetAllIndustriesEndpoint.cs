using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.MasterData.Application;
using Crm.MasterData.Application.Queries;
using Crm.MasterData.Contracts.Requests;
using Crm.MasterData.Contracts.Response;

namespace Crm.MasterData.Api.Endpoints.Industry;

public class GetAllIndustriesEndpoint : QueryGridEndpoint<GetAllIndustriesRequest,
    GetAllIndustriesQuery, IndustryModel, IndustryViewModel>
{
    public override string Route => Constants.IndustriesRoute;

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Industries");
    }
}
