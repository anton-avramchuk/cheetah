using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Candidates.Application;
using Crm.Candidates.Application.Queries;
using Crm.Candidates.Contracts.Requests;
using Crm.Candidates.Contracts.Response;

namespace Crm.Candidates.Api.Endpoints;

public class GetAllCandidateStagesEndpoint : QueryCollectionEndpoint<GetAllCandidateStagesRequest,
    GetAllCandidateStagesQuery, CandidateStageModel, CandidateStageViewModel>
{
    public override string Route => Constants.CandidateStageRoute;

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("CandidateStages");
    }
}
