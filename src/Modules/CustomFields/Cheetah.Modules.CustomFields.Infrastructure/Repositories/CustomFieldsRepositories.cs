using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.EntityFramework.Repositories;
using Cheetah.Modules.CustomFields.Domain.Entities;
using Cheetah.Modules.CustomFields.Infrastructure.Persistence;

namespace Cheetah.Modules.CustomFields.Infrastructure.Repositories;

[Export(LifetimeType.Scoped, typeof(IRepository<CustomFieldEntityType, Guid>))]
public sealed class CustomFieldEntityTypeRepository
    : EfRepository<CustomFieldsDbContext, CustomFieldEntityType, Guid>
{
    public CustomFieldEntityTypeRepository(CustomFieldsDbContext context) : base(context) { }
}

[Export(LifetimeType.Scoped, typeof(IRepository<CustomFieldDefinition, Guid>))]
public sealed class CustomFieldDefinitionRepository
    : EfRepository<CustomFieldsDbContext, CustomFieldDefinition, Guid>
{
    public CustomFieldDefinitionRepository(CustomFieldsDbContext context) : base(context) { }
}

[Export(LifetimeType.Scoped, typeof(IRepository<CustomFieldValueSet, Guid>))]
public sealed class CustomFieldValueSetRepository
    : EfRepository<CustomFieldsDbContext, CustomFieldValueSet, Guid>
{
    public CustomFieldValueSetRepository(CustomFieldsDbContext context) : base(context) { }
}
