using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.Recruitment.Contracts.Response;

namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/stack-items/{id:guid}", ApiMethod.GetOrNotFound, ResponseType = typeof(StackItemViewModel), ServiceName = "StackItems")]
public record GetStackItemByIdRequest([FromRoute] Guid Id) : ICrmRequest;
