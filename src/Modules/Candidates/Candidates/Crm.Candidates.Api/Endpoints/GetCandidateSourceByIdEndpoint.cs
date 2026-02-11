using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Candidates.Application;
using Crm.Candidates.Application.Queries;
using Crm.Candidates.Contracts.Requests;
using Crm.Candidates.Contracts.Response;

namespace Crm.Candidates.Api.Endpoints;

public class GetCandidateSourceByIdEndpoint : QueryOrNotFoundEndpoint<GetCandidateSourceByIdRequest, GetCandidateSourceByIdQuery,
    CandidateSourceModel, CandidateSourceViewModel>
{
    public override string Route => $"{Constants.CandidateSourceRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithName("GetCandidateSourceById").WithTags("CandidateSources");
    }
}
