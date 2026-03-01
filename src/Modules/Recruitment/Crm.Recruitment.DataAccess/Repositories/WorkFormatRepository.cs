using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Mapping.Core;
using Crm.Recruitment.Domain;
using Microsoft.Extensions.Logging;

namespace Crm.Recruitment.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<WorkFormat>), typeof(IRepository<WorkFormat, Guid>))]
public class WorkFormatRepository : EfGridRepository<RecruitmentDbContext, WorkFormat>
{
    public WorkFormatRepository(RecruitmentDbContext context, IObjectMapper mapper, ILogger<WorkFormatRepository> logger)
        : base(context, mapper, logger)
    {
    }
}
