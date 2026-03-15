using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.MasterData.Contracts.Requests;

[ApiRoute("api/industries/{id:guid}", ApiMethod.Delete, ServiceName = "Industry")]
public record DeleteIndustryRequest([FromRoute] Guid Id) : ICrmRequest;
