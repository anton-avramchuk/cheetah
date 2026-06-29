using Cheetah.AspNetCore.Blazor.Grid;
using Cheetah.Core.Domain;

namespace Cheetah.AspNetCore.Blazor.Tests.Grid;

public sealed class FakeEntity : Entity<Guid>
{
    public FakeEntity() { }
    public FakeEntity(Guid id) => Id = id;
}

public sealed class FakeGridViewModel : IHasId
{
    public Guid Id { get; set; }
    [GridColumn("Имя", order: 0)] public string Name { get; set; } = string.Empty;
}

public sealed class FakeDetailsViewModel
{
    public string Name { get; set; } = string.Empty;
}

public sealed class FakeCreateViewModel
{
    public string Name { get; set; } = string.Empty;
}
