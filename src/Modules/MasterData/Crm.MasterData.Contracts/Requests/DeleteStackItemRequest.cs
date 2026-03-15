using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.MasterData.Contracts.Requests;

[ApiRoute("api/stack-items/{id:guid}", ApiMethod.Delete, ServiceName = "StackItem")]
public record DeleteStackItemRequest([FromRoute] Guid Id) : ICrmRequest;
