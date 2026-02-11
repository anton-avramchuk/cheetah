using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Candidates.Application;
using Crm.Candidates.Application.Queries;
using Crm.Candidates.Contracts.Requests;
using Crm.Candidates.Contracts.Response;

namespace Crm.Candidates.Api.Endpoints;

public class GetCandidateApplicationByIdEndpoint : QueryOrNotFoundEndpoint<GetCandidateApplicationByIdRequest, GetCandidateApplicationByIdQuery,
    CandidateApplicationModel, CandidateApplicationViewModel>
{
    public override string Route => $"{Constants.CandidateApplicationRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithName("GetCandidateApplicationById").WithTags("CandidateApplications");
    }
}
