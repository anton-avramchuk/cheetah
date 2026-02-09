using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.Domain;

namespace Crm.Recruitment.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IRepository<User, Guid>))]
public class UserRepository : Cheetah.Core.EntityFramework.Repositories.EfRepository<RecruitmentDbContext, User>
{
    public UserRepository(RecruitmentDbContext context) : base(context)
    {
    }
}
