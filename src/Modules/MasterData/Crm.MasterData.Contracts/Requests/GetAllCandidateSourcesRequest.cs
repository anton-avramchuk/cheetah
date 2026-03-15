using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.MasterData.Contracts.Response;

namespace Crm.MasterData.Contracts.Requests;

[ApiRoute("api/candidate-sources", ApiMethod.GetGrid, ResponseType = typeof(CandidateSourceViewModel), ServiceName = "CandidateSource")]
public class GetAllCandidateSourcesRequest : GridRequest;
