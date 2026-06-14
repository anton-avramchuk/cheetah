using Cheetah.Modules.Calendar.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.Calendar.Client;

/// <summary>
/// Аккумулятор привязываемых типов сущностей, которые сервис регистрирует при старте.
/// Заполняется через <see cref="CalendarableTypeRegistrationExtensions.AddCalendarableEntityType"/>;
/// несколько вызовов суммируются.
/// </summary>
public sealed class CalendarableTypeRegistrationOptions
{
    public List<CalendarableEntityTypeRegistration> Items { get; } = new();
}

/// <summary>Настройки одного регистрируемого типа.</summary>
public sealed class CalendarableTypeBuilder
{
    public string? DefaultColor { get; set; }
    public bool AllowMultiplePerEntity { get; set; } = true;
}

public static class CalendarableTypeRegistrationExtensions
{
    /// <summary>
    /// Объявить тип сущности этого сервиса, к которому можно привязывать события.
    /// Будет отправлен в Calendar при старте.
    /// </summary>
    public static IServiceCollection AddCalendarableEntityType(
        this IServiceCollection services,
        string entityType,
        string displayName,
        Action<CalendarableTypeBuilder>? configure = null)
    {
        var builder = new CalendarableTypeBuilder();
        configure?.Invoke(builder);

        services.Configure<CalendarableTypeRegistrationOptions>(o => o.Items.Add(new CalendarableEntityTypeRegistration(
            entityType,
            displayName,
            builder.DefaultColor,
            builder.AllowMultiplePerEntity,
            OwnerService: null))); // проставляется из CalendarClientOptions.OwnerService при синхронизации

        return services;
    }
}
