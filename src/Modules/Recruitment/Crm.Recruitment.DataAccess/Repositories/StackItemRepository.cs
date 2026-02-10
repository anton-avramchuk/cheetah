using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.Domain;

namespace Crm.Recruitment.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IRepository<StackItem, Guid>))]
public class StackItemRepository : Cheetah.Core.EntityFramework.Repositories.EfRepository<RecruitmentDbContext, StackItem>
{
    public StackItemRepository(RecruitmentDbContext context) : base(context)
    {
    }
}
