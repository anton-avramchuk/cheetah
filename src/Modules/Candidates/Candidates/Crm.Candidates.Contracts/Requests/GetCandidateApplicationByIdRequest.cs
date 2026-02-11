using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.Candidates.Contracts.Response;

namespace Crm.Candidates.Contracts.Requests;

[ApiRoute("api/candidate-applications/{id:guid}", ApiMethod.GetOrNotFound, ResponseType = typeof(CandidateApplicationViewModel), ServiceName = "CandidateApplications")]
public record GetCandidateApplicationByIdRequest([FromRoute] Guid Id) : ICrmRequest;
