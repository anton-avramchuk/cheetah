using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.MasterData.Contracts.Requests;

public record GetLocationByIdRequest([FromRoute] Guid Id) : ICrmRequest;
