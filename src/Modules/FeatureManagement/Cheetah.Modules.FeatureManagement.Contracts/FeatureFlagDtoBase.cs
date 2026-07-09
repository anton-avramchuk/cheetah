using Cheetah.Contracts.Responses;
using Cheetah.FeatureManagement;

namespace Cheetah.Modules.FeatureManagement.Contracts;

/// <summary>
/// Базовый ViewModel флага (граница API). Абстрактен: наследник объявляет конкретный
/// <c>sealed record FeatureFlagDto : FeatureFlagDtoBase</c> и при необходимости добавляет свои поля
/// (например, <c>JiraTicket</c>, <c>OwnerTeam</c>). Точка расширяемости ViewModel.
/// </summary>
public abstract record FeatureFlagDtoBase : ICrmResponse
{
    public Guid Id { get; init; }
    public string Key { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public string OwnerService { get; init; } = null!;
    public string? ParentKey { get; init; }
    public bool Enabled { get; init; }
    public FeatureValueType ValueType { get; init; }
    public bool IsActive { get; init; }
    public IReadOnlyList<TargetingRuleDto> Rules { get; init; } = [];
    public IReadOnlyList<FeatureVariantDto> Variants { get; init; } = [];
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
