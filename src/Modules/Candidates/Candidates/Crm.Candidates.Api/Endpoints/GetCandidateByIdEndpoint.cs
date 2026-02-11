using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Candidates.Application;
using Crm.Candidates.Application.Queries;
using Crm.Candidates.Contracts.Requests;
using Crm.Candidates.Contracts.Response;

namespace Crm.Candidates.Api.Endpoints;

public class GetCandidateByIdEndpoint : QueryOrNotFoundEndpoint<GetCandidateByIdRequest, GetCandidateByIdQuery,
    CandidateModel, CandidateViewModel>
{
    public override string Route => $"{Constants.DefaultRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithName("GetCandidateById");
        config.WithTags("Candidates");
    }
}