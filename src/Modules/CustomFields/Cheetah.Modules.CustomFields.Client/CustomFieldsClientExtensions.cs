using Cheetah.Modules.CustomFields.Contracts;
using Cheetah.Modules.CustomFields.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.CustomFields.Client;

/// <summary>Опции HTTP-клиента CustomFields.</summary>
public sealed class CustomFieldsClientOptions
{
    /// <summary>Базовый адрес сервиса CustomFields.</summary>
    public string BaseUrl { get; set; } = "http://localhost";
}

/// <summary>Вклад одного потребителя в реестр типов (агрегируется hosted-сервисом).</summary>
public sealed record CustomFieldsRegistrationContribution(IReadOnlyList<CustomFieldEntityTypeDescriptor> Descriptors);

/// <summary>Настройки объявляемого расширяемого типа.</summary>
public sealed class CustomFieldTypeOptions
{
    public CustomFieldEntityIdType IdType { get; set; } = CustomFieldEntityIdType.Guid;
    public IReadOnlyList<PredefinedFieldDescriptor>? PredefinedFields { get; set; }
}

/// <summary>Билдер декларации расширяемых типов потребителем.</summary>
public interface ICustomFieldTypeRegistrationBuilder
{
    ICustomFieldTypeRegistrationBuilder Add(string key, string displayName, Action<CustomFieldTypeOptions>? configure = null);
}

/// <summary>Билдер клиента CustomFields.</summary>
public interface ICustomFieldsClientBuilder
{
    IServiceCollection Services { get; }
}

public static class CustomFieldsClientExtensions
{
    /// <summary>Регистрирует HTTP-клиент CustomFields + hosted-сервис регистрации типов при старте.</summary>
    public static ICustomFieldsClientBuilder AddCustomFieldsClient(
        this IServiceCollection services, Action<CustomFieldsClientOptions> configure)
    {
        var options = new CustomFieldsClientOptions();
        configure(options);

        services.AddHttpClient<ICustomFieldsClient, HttpCustomFieldsClient>(c =>
            c.BaseAddress = new Uri(options.BaseUrl));

        services.AddHostedService<CustomFieldsRegistrationSyncService>();
        return new Builder(services);
    }

    /// <summary>Декларирует расширяемые типы потребителя (синкаются в каталог при старте).</summary>
    public static ICustomFieldsClientBuilder RegisterCustomFieldTypes(
        this ICustomFieldsClientBuilder builder, Action<ICustomFieldTypeRegistrationBuilder> configure)
    {
        var reg = new RegistrationBuilder();
        configure(reg);
        builder.Services.AddSingleton(new CustomFieldsRegistrationContribution(reg.Descriptors));
        return builder;
    }

    private sealed class Builder(IServiceCollection services) : ICustomFieldsClientBuilder
    {
        public IServiceCollection Services { get; } = services;
    }

    private sealed class RegistrationBuilder : ICustomFieldTypeRegistrationBuilder
    {
        public List<CustomFieldEntityTypeDescriptor> Descriptors { get; } = new();

        public ICustomFieldTypeRegistrationBuilder Add(
            string key, string displayName, Action<CustomFieldTypeOptions>? configure = null)
        {
            var o = new CustomFieldTypeOptions();
            configure?.Invoke(o);
            var ownerService = key.Contains('.') ? key[..key.IndexOf('.')] : key;
            Descriptors.Add(new CustomFieldEntityTypeDescriptor(
                key, displayName, ownerService, o.IdType, o.PredefinedFields));
            return this;
        }
    }
}
