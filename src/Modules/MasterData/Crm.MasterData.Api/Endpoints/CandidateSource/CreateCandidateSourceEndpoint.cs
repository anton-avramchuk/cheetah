using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.MasterData.Application.Commands;
using Crm.MasterData.Contracts.Requests;

namespace Crm.MasterData.Api.Endpoints.CandidateSource;

public class CreateCandidateSourceEndpoint : CreateCommandEndpoint<CreateCandidateSourceRequest, CreateCandidateSourceCommand>
{
    public override string Route => Constants.CandidateSourcesRoute;
    public override string GetByIdRouteName => "GetCandidateSourceById";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("CandidateSources");
    }
}
