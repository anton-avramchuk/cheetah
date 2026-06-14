using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Tags.Application.Tags;
using Cheetah.Modules.Tags.Contracts.Tags;
using Cheetah.Modules.Tags.Domain.Entities;
using Cheetah.Modules.Tags.Domain.Specifications;

namespace Cheetah.Modules.Tags.Application.Assignments;

/// <summary>Тэги, назначенные конкретной сущности.</summary>
public sealed record GetEntityTagsQuery(string EntityType, Guid EntityId)
    : IQuery<IReadOnlyList<TagDto>>;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetEntityTagsQuery, IReadOnlyList<TagDto>>))]
public class GetEntityTagsQueryHandler : IQueryHandler<GetEntityTagsQuery, IReadOnlyList<TagDto>>
{
    private readonly IRepository<TagAssignment, Guid> _assignments;
    private readonly IRepository<Tag, Guid> _tags;

    public GetEntityTagsQueryHandler(IRepository<TagAssignment, Guid> assignments, IRepository<Tag, Guid> tags)
    {
        _assignments = assignments;
        _tags = tags;
    }

    public async ValueTask<IReadOnlyList<TagDto>> HandleAsync(GetEntityTagsQuery query, CancellationToken ct = default)
    {
        var assignments = await _assignments.GetAllAsync(
            new AssignmentsByEntitySpecification(query.EntityType, query.EntityId), ct);

        var tagIds = assignments.Select(a => a.TagId).Distinct().ToArray();
        if (tagIds.Length == 0)
            return Array.Empty<TagDto>();

        var tags = await _tags.GetAllAsync(new TagsByIdsSpecification(tagIds), ct);
        return tags
            .OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
            .Select(TagMapper.Map)
            .ToArray();
    }
}
