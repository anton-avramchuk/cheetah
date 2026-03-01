using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Mapping.Core;
using Crm.Recruitment.Domain;
using Microsoft.Extensions.Logging;

namespace Crm.Recruitment.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<Customer>), typeof(IRepository<Customer, Guid>))]
public class CustomerRepository : EfGridRepository<RecruitmentDbContext, Customer>
{
    public CustomerRepository(RecruitmentDbContext context, IObjectMapper mapper, ILogger<CustomerRepository> logger)
        : base(context, mapper, logger)
    {
    }
}
