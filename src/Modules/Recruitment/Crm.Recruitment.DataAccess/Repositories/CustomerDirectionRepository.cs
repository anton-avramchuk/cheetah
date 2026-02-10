using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.Domain;

namespace Crm.Recruitment.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IRepository<CustomerDirection, Guid>))]
public class CustomerDirectionRepository : Cheetah.Core.EntityFramework.Repositories.EfRepository<RecruitmentDbContext, CustomerDirection>
{
    public CustomerDirectionRepository(RecruitmentDbContext context) : base(context)
    {
    }
}
