using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Cheetah.Modules.Identity.Contracts.Response;

namespace Cheetah.Modules.Identity.Contracts.Requests;

[ApiRoute("api/users", ApiMethod.GetGrid, ResponseType = typeof(UserGridViewModel), ServiceName = "Users")]
public class GetUsersGridRequest : GridRequest;
