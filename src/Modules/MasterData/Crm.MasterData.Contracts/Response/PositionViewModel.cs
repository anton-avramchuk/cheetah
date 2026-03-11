using Cheetah.Contracts.Responses;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Contracts.Response;

public record PositionViewModel(Guid Id, string Name, Grade Grade) : ICrmResponse;
