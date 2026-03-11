using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.MasterData.Application.Commands;
using Crm.MasterData.Contracts.Requests;

namespace Crm.MasterData.Api.Endpoints.WorkFormat;

public class DeleteWorkFormatEndpoint : DeleteCommandEndpoint<DeleteWorkFormatRequest, DeleteWorkFormatCommand>
{
    public override string Route => $"{Constants.WorkFormatsRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("WorkFormats");
    }
}
