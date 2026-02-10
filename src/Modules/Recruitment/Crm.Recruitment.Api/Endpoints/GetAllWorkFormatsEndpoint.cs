using Cheetah.Backend.Endpoints.Http;
using Crm.Recruitment.Application;
using Crm.Recruitment.Application.Queries;
using Crm.Recruitment.Contracts.Requests;
using Crm.Recruitment.Contracts.Response;

namespace Crm.Recruitment.Api.Endpoints;

public class GetAllWorkFormatsEndpoint : QueryCollectionEndpoint<GetAllWorkFormatsRequest,
    GetAllWorkFormatsQuery, WorkFormatModel, WorkFormatViewModel>
{
    public override string Route => Constants.WorkFormatRoute;
}
