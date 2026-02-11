using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Candidates.Application.Commands;
using Crm.Candidates.Contracts.Requests;

namespace Crm.Candidates.Api.Endpoints;

public class DeleteCandidateSourceEndpoint : DeleteCommandEndpoint<DeleteCandidateSourceRequest, DeleteCandidateSourceCommand>
{
    public override string Route => $"{Constants.CandidateSourceRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("CandidateSources");
    }
}
