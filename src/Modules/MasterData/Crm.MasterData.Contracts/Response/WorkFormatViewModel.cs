using Cheetah.Contracts.Responses;

namespace Crm.MasterData.Contracts.Response;

public record WorkFormatViewModel(Guid Id, string Name) : ICrmResponse;
