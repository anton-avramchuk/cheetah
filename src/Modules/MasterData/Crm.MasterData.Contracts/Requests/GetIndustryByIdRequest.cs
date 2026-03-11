using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.MasterData.Contracts.Requests;

public record GetIndustryByIdRequest([FromRoute] Guid Id) : ICrmRequest;
