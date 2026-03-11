using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Mapping.Core;
using Crm.MasterData.Domain;
using Microsoft.Extensions.Logging;

namespace Crm.MasterData.DataAccess.Repositories;

[Export(LifetimeType.Scoped, typeof(IGridRepository<CandidateSource>), typeof(IRepository<CandidateSource, Guid>))]
public class CandidateSourceRepository : EfGridRepository<MasterDataDbContext, CandidateSource>
{
    public CandidateSourceRepository(MasterDataDbContext context, IObjectMapper mapper, ILogger<CandidateSourceRepository> logger)
        : base(context, mapper, logger)
    {
    }
}
