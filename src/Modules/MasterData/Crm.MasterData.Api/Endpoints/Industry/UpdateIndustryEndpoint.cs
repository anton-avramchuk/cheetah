using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.MasterData.Application.Commands;
using Crm.MasterData.Contracts.Requests;

namespace Crm.MasterData.Api.Endpoints.Industry;

public class UpdateIndustryEndpoint : UpdateCommandEndpoint<UpdateIndustryRequest, UpdateIndustryCommand>
{
    public override string Route => $"{Constants.IndustriesRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Industries");
    }
}
