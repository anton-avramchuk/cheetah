using Cheetah.Core.Attributes;
using Cheetah.Contracts.Responses;

namespace Crm.MasterData.Contracts.Response;

public record IndustryViewModel(Guid Id, [SyncHash] string Name) : ICrmResponse;
