namespace Cheetah.Modules.Tags.Contracts.Tags;

/// <summary>Создать тэг в словаре тенанта.</summary>
public sealed record CreateTagRequest(
    string Name,
    string? Color = null,
    string? Description = null,
    string? Group = null);
