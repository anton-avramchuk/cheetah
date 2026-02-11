using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.Candidates.Contracts.Response;

namespace Crm.Candidates.Contracts.Requests;

[ApiRoute("api/candidate-sources", ApiMethod.GetCollection, ResponseType = typeof(CandidateSourceViewModel), ServiceName = "CandidateSources")]
public record GetAllCandidateSourcesRequest : ICrmRequest;
