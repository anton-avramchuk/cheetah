using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Cheetah.Admin.Modules.Clients.Contracts.Requests;

public record GetTariffByIdRequest([FromRoute] Guid Id) : ICrmRequest;
