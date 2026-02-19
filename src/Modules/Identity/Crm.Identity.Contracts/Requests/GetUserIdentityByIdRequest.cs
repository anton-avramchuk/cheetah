using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.Identity.Contracts.Response;

namespace Crm.Identity.Contracts.Requests;

[ApiRoute("api/user-identities/{id:guid}", ApiMethod.GetOrNotFound, ResponseType = typeof(UserIdentityViewModel))]
public record GetUserIdentityByIdRequest([FromRoute] Guid Id) : ICrmRequest;