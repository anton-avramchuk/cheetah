using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.MasterData.Contracts.Response;

namespace Crm.MasterData.Contracts.Requests;

[ApiRoute("api/work-formats/{id:guid}", ApiMethod.GetOrNotFound, ResponseType = typeof(WorkFormatViewModel), ServiceName = "WorkFormat")]
public record GetWorkFormatByIdRequest([FromRoute] Guid Id) : ICrmRequest;
