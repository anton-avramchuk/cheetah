using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.MasterData.Application.Commands;
using Crm.MasterData.Contracts.Requests;

namespace Crm.MasterData.Api.Endpoints.Industry;

public class CreateIndustryEndpoint : CreateCommandEndpoint<CreateIndustryRequest, CreateIndustryCommand>
{
    public override string Route => Constants.IndustriesRoute;
    public override string GetByIdRouteName => "GetIndustryById";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Industries");
    }
}
