using Cheetah.Core.CQRS;

namespace Cheetah.Features.Application.Queries;

public record CheckFeatureQuery(Guid TenantId, string FeatureId) : IQuery<bool>;
