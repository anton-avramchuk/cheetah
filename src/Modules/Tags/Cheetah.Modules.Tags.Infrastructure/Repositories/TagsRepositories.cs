using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.EntityFramework.Repositories;
using Cheetah.Modules.Tags.Domain.Entities;
using Cheetah.Modules.Tags.Infrastructure.Persistence;

namespace Cheetah.Modules.Tags.Infrastructure.Repositories;

[Export(LifetimeType.Scoped, typeof(IRepository<TaggableEntityType, string>))]
public class TaggableEntityTypeRepository : EfRepository<TagsDbContext, TaggableEntityType, string>
{
    public TaggableEntityTypeRepository(TagsDbContext context) : base(context) { }
}

[Export(LifetimeType.Scoped, typeof(IRepository<Tag, Guid>))]
public class TagRepository : EfRepository<TagsDbContext, Tag, Guid>
{
    public TagRepository(TagsDbContext context) : base(context) { }
}

[Export(LifetimeType.Scoped, typeof(IRepository<TagAssignment, Guid>))]
public class TagAssignmentRepository : EfRepository<TagsDbContext, TagAssignment, Guid>
{
    public TagAssignmentRepository(TagsDbContext context) : base(context) { }
}
