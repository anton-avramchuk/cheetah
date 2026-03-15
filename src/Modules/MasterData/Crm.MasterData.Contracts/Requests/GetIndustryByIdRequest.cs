using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.MasterData.Contracts.Response;

namespace Crm.MasterData.Contracts.Requests;

[ApiRoute("api/industries/{id:guid}", ApiMethod.GetOrNotFound, ResponseType = typeof(IndustryViewModel), ServiceName = "Industry")]
public record GetIndustryByIdRequest([FromRoute] Guid Id) : ICrmRequest;
