using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.Recruitment.Contracts.Response;

namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/customer-directions/{id:guid}", ApiMethod.GetOrNotFound, ResponseType = typeof(CustomerDirectionViewModel), ServiceName = "CustomerDirections")]
public record GetCustomerDirectionByIdRequest([FromRoute] Guid Id) : ICrmRequest;
