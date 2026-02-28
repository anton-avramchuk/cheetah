using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.Recruitment.Contracts.Response;

namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/customer-directions", ApiMethod.GetGrid, ResponseType = typeof(CustomerDirectionViewModel), ServiceName = "CustomerDirections")]
public class GetAllCustomerDirectionsRequest : GridRequest;
