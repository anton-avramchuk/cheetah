using Cheetah.Core.CQRS;

namespace Cheetah.Tenants.Application.Commands;

public record CreateTenantCommand(
    string Name,
    string? Subdomain = null
) : ICommand<Guid>;
