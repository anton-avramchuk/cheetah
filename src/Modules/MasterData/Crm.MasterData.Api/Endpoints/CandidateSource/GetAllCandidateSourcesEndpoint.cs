using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.MasterData.Application;
using Crm.MasterData.Application.Queries;
using Crm.MasterData.Contracts.Requests;
using Crm.MasterData.Contracts.Response;

namespace Crm.MasterData.Api.Endpoints.CandidateSource;

public class GetAllCandidateSourcesEndpoint : QueryGridEndpoint<GetAllCandidateSourcesRequest,
    GetAllCandidateSourcesQuery, CandidateSourceModel, CandidateSourceViewModel>
{
    public override string Route => Constants.CandidateSourcesRoute;

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("CandidateSources");
    }
}
