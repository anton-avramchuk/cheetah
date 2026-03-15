using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.MasterData.Contracts.Response;

namespace Crm.MasterData.Contracts.Requests;

[ApiRoute("api/stack-items/{id:guid}", ApiMethod.GetOrNotFound, ResponseType = typeof(StackItemViewModel), ServiceName = "StackItem")]
public record GetStackItemByIdRequest([FromRoute] Guid Id) : ICrmRequest;
