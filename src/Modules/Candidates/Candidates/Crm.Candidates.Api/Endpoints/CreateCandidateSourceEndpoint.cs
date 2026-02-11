using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Candidates.Application.Commands;
using Crm.Candidates.Contracts.Requests;

namespace Crm.Candidates.Api.Endpoints;

public class CreateCandidateSourceEndpoint : CreateCommandEndpoint<CreateCandidateSourceRequest, CreateCandidateSourceCommand>
{
    public override string Route => Constants.CandidateSourceRoute;
    public override string GetByIdRouteName => "GetCandidateSourceById";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("CandidateSources");
    }
}
