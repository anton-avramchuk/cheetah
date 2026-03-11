using Cheetah.Contracts.Responses;

namespace Crm.MasterData.Contracts.Response;

public record StackItemViewModel(Guid Id, string Name, string? Description) : ICrmResponse;