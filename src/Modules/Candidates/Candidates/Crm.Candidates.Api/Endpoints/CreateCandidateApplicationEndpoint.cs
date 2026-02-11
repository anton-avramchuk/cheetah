using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Candidates.Application.Commands;
using Crm.Candidates.Contracts.Requests;

namespace Crm.Candidates.Api.Endpoints;

public class CreateCandidateApplicationEndpoint : CreateCommandEndpoint<CreateCandidateApplicationRequest, CreateCandidateApplicationCommand>
{
    public override string Route => Constants.CandidateApplicationRoute;
    public override string GetByIdRouteName => "GetCandidateApplicationById";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("CandidateApplications");
    }
}
