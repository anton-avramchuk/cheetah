using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Cheetah.Modules.Identity.Contracts.Response;

namespace Cheetah.Modules.Identity.Contracts.Requests;

[ApiRoute("api/users/{id:guid}", ApiMethod.GetOrNotFound, ResponseType = typeof(UserDetailViewModel), ServiceName = "Users")]
public record GetUserByIdRequest([FromRoute] Guid Id) : ICrmRequest;
