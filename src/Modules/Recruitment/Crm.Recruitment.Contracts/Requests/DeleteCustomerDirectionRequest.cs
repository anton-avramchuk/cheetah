using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Recruitment.Contracts.Requests;

[ApiRoute("api/customer-directions/{id:guid}", ApiMethod.Delete, ServiceName = "CustomerDirections")]
public record DeleteCustomerDirectionRequest([FromRoute] Guid Id) : ICrmRequest;
