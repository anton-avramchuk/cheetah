using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.Domain;

namespace Crm.Recruitment.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IRepository<WorkFormat, Guid>))]
public class WorkFormatRepository : Cheetah.Core.EntityFramework.Repositories.EfRepository<RecruitmentDbContext, WorkFormat>
{
    public WorkFormatRepository(RecruitmentDbContext context) : base(context)
    {
    }
}
