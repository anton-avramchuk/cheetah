using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.Recruitment.Contracts.Response;

namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/customer-directions", ApiMethod.GetCollection, ResponseType = typeof(CustomerDirectionViewModel), ServiceName = "CustomerDirections")]
public record GetAllCustomerDirectionsRequest : ICrmRequest;
