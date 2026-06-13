using Cheetah.Core.Dapper.Dialect;
using Cheetah.Core.Dapper.Mapping;
using Cheetah.Core.Domain;

namespace Cheetah.Core.Dapper.Tests;

/// <summary>Entity with an explicit map (used to test column-name overrides).</summary>
public sealed class ParserEntity : Entity<Guid>
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public bool IsActive { get; set; }
    public decimal? Score { get; set; }
    public string? Nickname { get; set; }
}

/// <summary>Explicit map overriding the column name of <see cref="ParserEntity.Nickname"/>.</summary>
public sealed class ParserEntityMap : DapperEntityMap<ParserEntity>
{
    public ParserEntityMap()
    {
        ToTable("ParserEntities");
        HasKey(x => x.Id);
        Column(x => x.Nickname!, "nick_name");
    }
}

/// <summary>Entity with no explicit map (used to test convention fallback).</summary>
public sealed class ConventionEntity : Entity<Guid>
{
    public string Title { get; set; } = string.Empty;
}

/// <summary>ANSI dialect for assertions (double quotes, <c>@</c> parameters).</summary>
public sealed class TestDialect : SqlDialectBase;
