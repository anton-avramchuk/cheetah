using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Candidates.Application;
using Crm.Candidates.Application.Queries;
using Crm.Candidates.Contracts.Requests;
using Crm.Candidates.Contracts.Response;

namespace Crm.Candidates.Api.Endpoints;

public class GetAllCandidateApplicationsEndpoint : QueryGridEndpoint<GetAllCandidateApplicationsRequest,
    GetAllCandidateApplicationsQuery, CandidateApplicationModel, CandidateApplicationViewModel>
{
    public override string Route => Constants.CandidateApplicationRoute;

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("CandidateApplications");
    }
}
