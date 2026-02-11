using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Candidates.Application.Commands;
using Crm.Candidates.Contracts.Requests;

namespace Crm.Candidates.Api.Endpoints;

public class UpdateCandidateEndpoint : UpdateCommandEndpoint<UpdateCandidateRequest, UpdateCandidateCommand>
{
    public override string Route => $"{Constants.DefaultRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Candidates");
    }
}