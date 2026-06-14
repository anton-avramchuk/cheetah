using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Specification;
using Cheetah.Modules.Tags.Contracts.Tags;
using Cheetah.Modules.Tags.Domain.Entities;
using Cheetah.Modules.Tags.Domain.Specifications;

namespace Cheetah.Modules.Tags.Application.Tags;

/// <summary>Список тэгов словаря (опц. фильтр по группе).</summary>
public sealed record ListTagsQuery(string? Group = null) : IQuery<IReadOnlyList<TagDto>>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<ListTagsQuery, IReadOnlyList<TagDto>>))]
public class ListTagsQueryHandler : IQueryHandler<ListTagsQuery, IReadOnlyList<TagDto>>
{
    private readonly IRepository<Tag, Guid> _repository;

    public ListTagsQueryHandler(IRepository<Tag, Guid> repository) => _repository = repository;

    public async ValueTask<IReadOnlyList<TagDto>> HandleAsync(ListTagsQuery query, CancellationToken ct = default)
    {
        ISpecification<Tag>? spec = string.IsNullOrEmpty(query.Group)
            ? null
            : new TagsByGroupSpecification(query.Group);

        var items = await _repository.GetAllAsync(spec, ct);
        return items
            .OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
            .Select(TagMapper.Map)
            .ToArray();
    }
}
