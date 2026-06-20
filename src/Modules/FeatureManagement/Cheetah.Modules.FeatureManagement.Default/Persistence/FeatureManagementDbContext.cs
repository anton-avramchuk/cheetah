using Cheetah.Modules.FeatureManagement.Default.Entities;
using Cheetah.Modules.FeatureManagement.Default.Persistence.Configurations;
using Cheetah.Modules.FeatureManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.FeatureManagement.Default.Persistence;

/// <summary>Конкретный DbContext «из коробки» поверх абстрактной базы шаблона.</summary>
public sealed class FeatureManagementDbContext : FeatureManagementDbContextBase<FeatureManagementDbContext, FeatureFlag>
{
    public FeatureManagementDbContext(DbContextOptions<FeatureManagementDbContext> options) : base(options)
    {
    }

    protected override IEntityTypeConfiguration<FeatureFlag> CreateFlagConfiguration()
        => new FeatureFlagConfiguration();
}
