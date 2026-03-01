using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Mapping.Core;
using Crm.Recruitment.Domain;
using Microsoft.Extensions.Logging;

namespace Crm.Recruitment.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<VacancyState>), typeof(IRepository<VacancyState, Guid>))]
public class VacancyStateRepository : EfGridRepository<RecruitmentDbContext, VacancyState>
{
    public VacancyStateRepository(RecruitmentDbContext context, IObjectMapper mapper, ILogger<VacancyStateRepository> logger)
        : base(context, mapper, logger)
    {
    }
}
