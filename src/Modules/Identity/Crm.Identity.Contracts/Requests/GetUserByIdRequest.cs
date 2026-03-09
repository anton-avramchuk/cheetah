using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.Identity.Contracts.Response;

namespace Crm.Identity.Contracts.Requests;

[ApiRoute("api/users/{id:guid}", ApiMethod.GetOrNotFound, ResponseType = typeof(UserDetailViewModel), ServiceName = "Users")]
public record GetUserByIdRequest([FromRoute] Guid Id) : ICrmRequest;
