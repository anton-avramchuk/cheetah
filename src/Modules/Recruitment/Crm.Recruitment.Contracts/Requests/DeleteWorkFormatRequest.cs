using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Crm.Recruitment.Contracts.Requests;

public record DeleteWorkFormatRequest([FromRoute] Guid Id) : ICrmRequest;
