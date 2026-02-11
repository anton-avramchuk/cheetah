using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.Candidates.Contracts.Response;

namespace Crm.Candidates.Contracts.Requests;

[ApiRoute("api/candidate-stages", ApiMethod.GetCollection, ResponseType = typeof(CandidateStageViewModel), ServiceName = "CandidateStages")]
public record GetAllCandidateStagesRequest : ICrmRequest;
