using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.Candidates.Contracts.Response;

namespace Crm.Candidates.Contracts.Requests;

[ApiRoute("api/candidate-applications", ApiMethod.GetGrid, ResponseType = typeof(CandidateApplicationViewModel), ServiceName = "CandidateApplications")]
public class GetAllCandidateApplicationsRequest : GridRequest;
