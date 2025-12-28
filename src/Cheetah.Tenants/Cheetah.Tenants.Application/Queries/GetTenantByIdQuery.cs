using Cheetah.Core.CQRS;
using Cheetah.Tenants.Domain.Entities;

namespace Cheetah.Tenants.Application.Queries;

public record GetTenantByIdQuery(Guid TenantId) : IQuery<Tenant?>;
