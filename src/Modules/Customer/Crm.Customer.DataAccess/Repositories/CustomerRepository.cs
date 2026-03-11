using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using CustomerEntity = global::Crm.Customer.Domain.Customer;
using Cheetah.Mapping.Core;
using Microsoft.Extensions.Logging;

namespace Crm.Customer.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<CustomerEntity>), typeof(IRepository<CustomerEntity, Guid>))]
public class CustomerRepository : EfGridRepository<CustomerDbContext, CustomerEntity>
{
    public CustomerRepository(CustomerDbContext context, IObjectMapper mapper, ILogger<CustomerRepository> logger)
        : base(context, mapper, logger)
    {
    }
}
