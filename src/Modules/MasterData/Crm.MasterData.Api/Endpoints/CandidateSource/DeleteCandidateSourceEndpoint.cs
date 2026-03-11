using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.MasterData.Application.Commands;
using Crm.MasterData.Contracts.Requests;

namespace Crm.MasterData.Api.Endpoints.CandidateSource;

public class DeleteCandidateSourceEndpoint : DeleteCommandEndpoint<DeleteCandidateSourceRequest, DeleteCandidateSourceCommand>
{
    public override string Route => $"{Constants.CandidateSourcesRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("CandidateSources");
    }
}
