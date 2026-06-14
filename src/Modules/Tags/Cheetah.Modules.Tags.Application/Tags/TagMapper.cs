using Cheetah.Modules.Tags.Contracts.Tags;
using Cheetah.Modules.Tags.Domain.Entities;

namespace Cheetah.Modules.Tags.Application.Tags;

internal static class TagMapper
{
    public static TagDto Map(Tag t)
        => new(t.Id, t.Name, t.Slug, t.Color, t.Description, t.Group);
}
