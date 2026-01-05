using Cheetah.Core.CQRS;

namespace Cheetah.Features.Application.Commands;

public record DisableFeatureCommand(
    Guid TenantId,
    string FeatureId
) : ICommand;
