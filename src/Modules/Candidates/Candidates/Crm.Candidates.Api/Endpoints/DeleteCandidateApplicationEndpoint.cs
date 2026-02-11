using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Candidates.Application.Commands;
using Crm.Candidates.Contracts.Requests;

namespace Crm.Candidates.Api.Endpoints;

public class DeleteCandidateApplicationEndpoint : DeleteCommandEndpoint<DeleteCandidateApplicationRequest, DeleteCandidateApplicationCommand>
{
    public override string Route => $"{Constants.CandidateApplicationRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("CandidateApplications");
    }
}
