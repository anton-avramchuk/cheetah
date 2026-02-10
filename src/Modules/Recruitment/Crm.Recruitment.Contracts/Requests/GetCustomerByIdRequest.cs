using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Recruitment.Contracts.Requests;

public record GetCustomerByIdRequest([FromRoute] Guid Id) : ICrmRequest;
