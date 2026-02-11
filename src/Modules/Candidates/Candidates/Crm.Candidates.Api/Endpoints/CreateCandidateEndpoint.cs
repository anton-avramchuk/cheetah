using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Candidates.Application.Commands;
using Crm.Candidates.Contracts.Requests;

namespace Crm.Candidates.Api.Endpoints;

public class CreateCandidateEndpoint : CreateCommandEndpoint<CreateCandidateRequest, CreateCandidateCommand>
{
    public override string Route => Constants.DefaultRoute;

    public override string GetByIdRouteName => "GetCandidateById";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Candidates");
    }
}