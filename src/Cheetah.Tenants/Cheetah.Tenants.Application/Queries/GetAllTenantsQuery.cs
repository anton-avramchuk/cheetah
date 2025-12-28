using Cheetah.Core.CQRS;
using Cheetah.Tenants.Domain.Entities;

namespace Cheetah.Tenants.Application.Queries;

public record GetAllTenantsQuery : IQuery<IReadOnlyList<Tenant>>;
