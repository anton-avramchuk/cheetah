using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Identity.Contracts.Requests;

[ApiRoute("api/users/{id:guid}", ApiMethod.Delete, ServiceName = "Users")]
public record DeleteUserRequest([FromRoute] Guid Id) : ICrmRequest;
