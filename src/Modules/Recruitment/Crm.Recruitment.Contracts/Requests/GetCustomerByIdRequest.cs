using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.Recruitment.Contracts.Response;

namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/customers/{id:guid}", ApiMethod.GetOrNotFound, ResponseType = typeof(CustomerViewModel), ServiceName = "Customers")]
public record GetCustomerByIdRequest([FromRoute] Guid Id) : ICrmRequest;
