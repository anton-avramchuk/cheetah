using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.Domain;

namespace Crm.Recruitment.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IRepository<VacancyRole, Guid>))]
public class VacancyRoleRepository : Cheetah.Core.EntityFramework.Repositories.EfRepository<RecruitmentDbContext, VacancyRole>
{
    public VacancyRoleRepository(RecruitmentDbContext context) : base(context)
    {
    }
}
