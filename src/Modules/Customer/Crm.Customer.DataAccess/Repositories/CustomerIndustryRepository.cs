using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.EntityFramework.Repositories;
using Crm.Customer.Domain;

namespace Crm.Customer.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IRepository<CustomerIndustry, Guid>))]
public class CustomerIndustryRepository : EfRepository<CustomerDbContext, CustomerIndustry>
{
    public CustomerIndustryRepository(CustomerDbContext context) : base(context)
    {
    }
}
