using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.Identity.Contracts.Response;

namespace Crm.Identity.Contracts.Requests;

[ApiRoute("api/users", ApiMethod.GetGrid, ResponseType = typeof(UserViewModel), ServiceName = "Users")]
public class GetAllUsersRequest : GridRequest;
