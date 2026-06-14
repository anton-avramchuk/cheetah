using Cheetah.Modules.Tags.Contracts.Registry;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.Tags.Client;

/// <summary>
/// Аккумулятор применимых типов сущностей, которые сервис регистрирует при старте.
/// Заполняется через <see cref="TaggableTypeRegistrationExtensions.AddTaggableEntityType"/>;
/// несколько вызовов суммируются (как любой <c>Configure</c>).
/// </summary>
public sealed class TaggableTypeRegistrationOptions
{
    public List<TaggableEntityTypeDto> Items { get; } = new();
}

/// <summary>Настройки одного регистрируемого типа.</summary>
public sealed class TaggableTypeBuilder
{
    public int? MaxTagsPerEntity { get; set; }
    public bool AllowAdHocTags { get; set; }
    public List<string> AllowedGroups { get; } = new();
}

public static class TaggableTypeRegistrationExtensions
{
    /// <summary>
    /// Объявить применимый к тэгам тип сущности этого сервиса. Будет отправлен в Tags при старте.
    /// </summary>
    public static IServiceCollection AddTaggableEntityType(
        this IServiceCollection services,
        string key,
        string displayName,
        Action<TaggableTypeBuilder>? configure = null)
    {
        var builder = new TaggableTypeBuilder();
        configure?.Invoke(builder);

        services.Configure<TaggableTypeRegistrationOptions>(o => o.Items.Add(new TaggableEntityTypeDto(
            key,
            displayName,
            OwnerService: "", // проставляется из TagsClientOptions.OwnerService при синхронизации
            builder.MaxTagsPerEntity,
            builder.AllowAdHocTags,
            builder.AllowedGroups.ToArray())));

        return services;
    }
}
