using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Customer.Contracts.Requests;

public record GetCustomerByIdRequest([FromRoute] Guid Id) : ICrmRequest;