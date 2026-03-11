using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Crm.Customer.Domain;
using Cheetah.Mapping.Core;
using Microsoft.Extensions.Logging;

namespace Crm.Customer.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<Customer>), typeof(IRepository<Customer, Guid>))]
public class CustomerRepository : EfGridRepository<CustomerDbContext, Customer>
{
    public CustomerRepository(CustomerDbContext context, IObjectMapper mapper, ILogger<CustomerRepository> logger)
        : base(context, mapper, logger)
    {
    }
}