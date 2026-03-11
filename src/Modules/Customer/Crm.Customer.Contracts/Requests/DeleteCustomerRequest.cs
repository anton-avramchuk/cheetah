using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Customer.Contracts.Requests;

public record DeleteCustomerRequest([FromRoute] Guid Id) : ICrmRequest;