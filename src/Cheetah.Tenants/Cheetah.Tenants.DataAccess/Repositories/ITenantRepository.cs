using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Tenants.Domain.Entities;

namespace Cheetah.Tenants.DataAccess.Repositories;

public interface ITenantRepository : IRepository<Tenant, Guid>, IReadOnlyRepository<Tenant, Guid>;