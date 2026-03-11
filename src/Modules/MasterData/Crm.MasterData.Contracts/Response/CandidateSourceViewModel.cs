using Cheetah.Contracts.Responses;

namespace Crm.MasterData.Contracts.Response;

public record CandidateSourceViewModel(Guid Id, string Name) : ICrmResponse;
