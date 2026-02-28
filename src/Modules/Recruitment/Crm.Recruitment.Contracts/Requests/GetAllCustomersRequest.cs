using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.Recruitment.Contracts.Response;

namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/customers", ApiMethod.GetGrid, ResponseType = typeof(CustomerViewModel), ServiceName = "Customers")]
public class GetAllCustomersRequest : GridRequest;
