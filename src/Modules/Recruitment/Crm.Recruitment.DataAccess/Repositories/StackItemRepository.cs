using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Mapping.Core;
using Crm.Recruitment.Domain;
using Microsoft.Extensions.Logging;

namespace Crm.Recruitment.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<StackItem>), typeof(IRepository<StackItem, Guid>))]
public class StackItemRepository : EfGridRepository<RecruitmentDbContext, StackItem>
{
    public StackItemRepository(RecruitmentDbContext context, IObjectMapper mapper, ILogger<StackItemRepository> logger)
        : base(context, mapper, logger)
    {
    }
}
