using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Candidates.Application.Commands;
using Crm.Candidates.Contracts.Requests;

namespace Crm.Candidates.Api.Endpoints;

public class CreateCandidateStageEndpoint : CreateCommandEndpoint<CreateCandidateStageRequest, CreateCandidateStageCommand>
{
    public override string Route => Constants.CandidateStageRoute;
    public override string GetByIdRouteName => "GetCandidateStageById";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("CandidateStages");
    }
}
