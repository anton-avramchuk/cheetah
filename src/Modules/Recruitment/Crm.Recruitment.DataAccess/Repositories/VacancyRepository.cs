using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Crm.Recruitment.Domain;
using Cheetah.Mapping.Core;
using Microsoft.Extensions.Logging;

namespace Crm.Recruitment.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<Vacancy>), typeof(IRepository<Vacancy, Guid>))]
public class VacancyRepository : EfGridRepository<RecruitmentDbContext, Vacancy>
{
    public VacancyRepository(RecruitmentDbContext context, IObjectMapper mapper, ILogger<VacancyRepository> logger)
        : base(context, mapper, logger)
    {
    }
}
