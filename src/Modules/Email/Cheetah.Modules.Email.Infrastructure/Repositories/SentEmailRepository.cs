using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.EntityFramework.Repositories;
using Cheetah.Modules.Email.Domain.Entities;
using Cheetah.Modules.Email.Infrastructure.Persistence;

namespace Cheetah.Modules.Email.Infrastructure.Repositories;

[Export(LifetimeType.Scoped, typeof(IRepository<SentEmail, Guid>))]
public class SentEmailRepository : EfRepository<EmailDbContext, SentEmail, Guid>
{
    public SentEmailRepository(EmailDbContext context) : base(context) { }
}
