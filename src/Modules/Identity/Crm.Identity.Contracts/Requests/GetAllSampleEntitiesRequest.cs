using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.Identity.Contracts.Response;

namespace Crm.Identity.Contracts.Requests;

[ApiRoute("api/user-identities", ApiMethod.GetGrid, ResponseType = typeof(UserIdentityViewModel))]
public class GetAllSampleEntitiesRequest : GridRequest;