using Cheetah.Core.CQRS;
using Cheetah.Tenants.Domain.Entities;

namespace Cheetah.Tenants.Application.Queries;

public record GetTenantBySubdomainQuery(string Subdomain) : IQuery<Tenant?>;
