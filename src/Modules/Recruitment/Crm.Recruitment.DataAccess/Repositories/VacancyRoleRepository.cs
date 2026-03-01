using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Mapping.Core;
using Crm.Recruitment.Domain;
using Microsoft.Extensions.Logging;

namespace Crm.Recruitment.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<VacancyRole>), typeof(IRepository<VacancyRole, Guid>))]
public class VacancyRoleRepository : EfGridRepository<RecruitmentDbContext, VacancyRole>
{
    public VacancyRoleRepository(RecruitmentDbContext context, IObjectMapper mapper, ILogger<VacancyRoleRepository> logger)
        : base(context, mapper, logger)
    {
    }
}
