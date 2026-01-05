using Cheetah.Core.CQRS;

namespace Cheetah.Features.Application.Commands;

public record CreateFeatureCommand(
    string Id,
    string DisplayName,
    string? Description = null,
    bool IsEnabledByDefault = false,
    string? Group = null
) : ICommand<string>;
