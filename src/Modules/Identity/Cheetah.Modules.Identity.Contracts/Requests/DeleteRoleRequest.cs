using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;

namespace Cheetah.Modules.Identity.Contracts.Requests;

public record DeleteRoleRequest([FromRoute] Guid Id) : ICrmRequest;
