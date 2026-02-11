using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/stack-items/{id:guid}", ApiMethod.Delete, ServiceName = "StackItems")]
public record DeleteStackItemRequest([FromRoute] Guid Id) : ICrmRequest;
