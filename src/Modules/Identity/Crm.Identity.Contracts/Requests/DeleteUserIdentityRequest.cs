using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Identity.Contracts.Requests;

[ApiRoute("api/user-identities/{id:guid}", ApiMethod.Delete)]
public record DeleteUserIdentityRequest([FromRoute] Guid Id) : ICrmRequest;