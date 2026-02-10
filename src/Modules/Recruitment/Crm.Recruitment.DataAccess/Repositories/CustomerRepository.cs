using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.Domain;

namespace Crm.Recruitment.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IRepository<Customer, Guid>))]
public class CustomerRepository : Cheetah.Core.EntityFramework.Repositories.EfRepository<RecruitmentDbContext, Customer>
{
    public CustomerRepository(RecruitmentDbContext context) : base(context)
    {
    }
}
