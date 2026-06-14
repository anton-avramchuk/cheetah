namespace Cheetah.Modules.Tags.Contracts.Tags;

/// <summary>Тэг словаря (ViewModel для чтения).</summary>
public sealed record TagDto(
    Guid Id,
    string Name,
    string Slug,
    string? Color,
    string? Description,
    string? Group);
