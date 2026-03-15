using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.MasterData.Contracts.Requests;

[ApiRoute("api/work-formats/{id:guid}", ApiMethod.Delete, ServiceName = "WorkFormat")]
public record DeleteWorkFormatRequest([FromRoute] Guid Id) : ICrmRequest;
