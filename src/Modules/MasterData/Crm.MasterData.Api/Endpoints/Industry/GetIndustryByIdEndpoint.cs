using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.MasterData.Application;
using Crm.MasterData.Application.Queries;
using Crm.MasterData.Contracts.Requests;
using Crm.MasterData.Contracts.Response;

namespace Crm.MasterData.Api.Endpoints.Industry;

public class GetIndustryByIdEndpoint : QueryOrNotFoundEndpoint<GetIndustryByIdRequest, GetIndustryByIdQuery,
    IndustryModel, IndustryViewModel>
{
    public override string Route => $"{Constants.IndustriesRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithName("GetIndustryById");
        config.WithTags("Industries");
    }
}
