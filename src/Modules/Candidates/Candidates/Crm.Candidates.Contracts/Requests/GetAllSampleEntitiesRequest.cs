using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.Candidates.Contracts.Response;

namespace Crm.Candidates.Contracts.Requests;

[ApiRoute("api/candidates", ApiMethod.GetGrid, ResponseType = typeof(CandidateViewModel))]
public class GetAllSampleEntitiesRequest : GridRequest;