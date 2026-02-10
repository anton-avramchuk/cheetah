using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Recruitment.Domain;

namespace Crm.Recruitment.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IRepository<Position, Guid>))]
public class PositionRepository : Cheetah.Core.EntityFramework.Repositories.EfRepository<RecruitmentDbContext, Position>
{
    public PositionRepository(RecruitmentDbContext context) : base(context)
    {
    }
}
