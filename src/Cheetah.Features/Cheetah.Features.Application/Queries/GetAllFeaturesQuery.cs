using Cheetah.Core.CQRS;
using Cheetah.Features.Domain.Entities;

namespace Cheetah.Features.Application.Queries;

public record GetAllFeaturesQuery : IQuery<IReadOnlyList<Feature>>;
