using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/customers/{id:guid}", ApiMethod.Delete, ServiceName = "Customers")]
public record DeleteCustomerRequest([FromRoute] Guid Id) : ICrmRequest;
