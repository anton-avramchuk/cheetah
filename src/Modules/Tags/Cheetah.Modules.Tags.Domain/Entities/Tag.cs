using Cheetah.Core.Domain;
using Cheetah.Modules.Tags.DomainEvents;

namespace Cheetah.Modules.Tags.Domain.Entities;

/// <summary>
/// Тэг словаря. Глобален; применимость к конкретным типам сущностей ограничивается
/// через <see cref="TaggableEntityType.AllowedGroups"/> и поле <see cref="Group"/>.
/// </summary>
public class Tag : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string? Color { get; private set; }
    public string? Description { get; private set; }
    public string? Group { get; private set; }

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    private Tag() { } // EF

    public static Tag Create(string name, string? color, string? description, string? group)
    {
        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Color = color,
            Description = description,
            Group = string.IsNullOrWhiteSpace(group) ? null : group
        };
        tag.SetName(name);
        tag.AddDomainEvent(new TagCreatedEvent(tag.Id, tag.Name));
        return tag;
    }

    public void Rename(string name) => SetName(name);

    public void Recolor(string? color) => Color = color;

    public void Describe(string? description) => Description = description;

    private void SetName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        Slug = Slugify(name);
    }

    private static string Slugify(string value)
    {
        var chars = value.Trim().ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray();
        var slug = new string(chars);
        while (slug.Contains("--"))
            slug = slug.Replace("--", "-");
        return slug.Trim('-');
    }
}
