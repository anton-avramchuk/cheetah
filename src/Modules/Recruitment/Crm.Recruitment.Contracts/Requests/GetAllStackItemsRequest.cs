using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.Recruitment.Contracts.Response;

namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/stack-items", ApiMethod.GetCollection, ResponseType = typeof(StackItemViewModel), ServiceName = "StackItems")]
public record GetAllStackItemsRequest : ICrmRequest;
