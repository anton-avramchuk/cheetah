using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.Domain;

namespace Crm.Recruitment.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IRepository<VacancyState, Guid>))]
public class VacancyStateRepository : Cheetah.Core.EntityFramework.Repositories.EfRepository<RecruitmentDbContext, VacancyState>
{
    public VacancyStateRepository(RecruitmentDbContext context) : base(context)
    {
    }
}
