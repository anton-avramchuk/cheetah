using Cheetah.Core.CQRS;

namespace Cheetah.Features.Application.Commands;

public record EnableFeatureCommand(
    Guid TenantId,
    string FeatureId
) : ICommand;
