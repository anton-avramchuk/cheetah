using Cheetah.Contracts.Responses;

namespace Crm.MasterData.Contracts.Response;

public record IndustryViewModel(Guid Id, string Name) : ICrmResponse;
