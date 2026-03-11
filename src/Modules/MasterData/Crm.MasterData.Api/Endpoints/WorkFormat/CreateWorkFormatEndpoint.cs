using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.MasterData.Application.Commands;
using Crm.MasterData.Contracts.Requests;

namespace Crm.MasterData.Api.Endpoints.WorkFormat;

public class CreateWorkFormatEndpoint : CreateCommandEndpoint<CreateWorkFormatRequest, CreateWorkFormatCommand>
{
    public override string Route => Constants.WorkFormatsRoute;
    public override string GetByIdRouteName => "GetWorkFormatById";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("WorkFormats");
    }
}
