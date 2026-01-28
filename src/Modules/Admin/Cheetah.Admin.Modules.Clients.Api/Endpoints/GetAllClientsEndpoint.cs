using Cheetah.Admin.Modules.Clients.Application.Queries;
using Cheetah.Admin.Modules.Clients.Contracts.Requests;
using Cheetah.Admin.Modules.Clients.Contracts.Response;
using Cheetah.Backend.Endpoints.Http;

namespace Cheetah.Admin.Modules.Clients.Api.Endpoints;

public class GetAllClientsEndpoint:QueryCollectionEndpoint<GetAllClientsRequest,GetAllClientsQuery,ClientModel,ClientViewModel>
{
    public override string Route => Constants.DefaultRoute;
}