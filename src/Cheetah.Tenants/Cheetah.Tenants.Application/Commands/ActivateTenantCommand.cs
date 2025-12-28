using Cheetah.Core.CQRS;

namespace Cheetah.Tenants.Application.Commands;

public record ActivateTenantCommand(Guid TenantId) : ICommand;
