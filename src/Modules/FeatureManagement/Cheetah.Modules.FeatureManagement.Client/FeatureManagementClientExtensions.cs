using Cheetah.FeatureManagement;
using Cheetah.Modules.FeatureManagement.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cheetah.Modules.FeatureManagement.Client;

/// <summary>Опции HTTP-клиента каталога фич-флагов.</summary>
public sealed class FeatureManagementClientOptions
{
    /// <summary>Базовый адрес сервиса FeatureManagement.</summary>
    public string BaseUrl { get; set; } = "http://localhost";
}

/// <summary>Вклад одного потребителя в реестр флагов (агрегируется hosted-сервисом).</summary>
public sealed record FeatureRegistrationContribution(IReadOnlyList<FeatureDefinitionDescriptor> Descriptors);

/// <summary>Билдер декларации флагов потребителем.</summary>
public interface IFeatureRegistrationBuilder
{
    /// <summary>Объявить флаг. <c>OwnerService</c> выводится из префикса ключа <c>"{service}.{feature}"</c>.</summary>
    IFeatureRegistrationBuilder Add(string key, string name, Action<FeatureDescriptorOptions>? configure = null);
}

/// <summary>Настройки объявляемого флага.</summary>
public sealed class FeatureDescriptorOptions
{
    public FeatureValueType ValueType { get; set; } = FeatureValueType.Bool;
    public string? Description { get; set; }
    public IReadOnlyList<string>? Variants { get; set; }
}

/// <summary>Билдер клиента FeatureManagement для дальнейшей настройки (реплика, флаги).</summary>
public interface IFeatureManagementClientBuilder
{
    IServiceCollection Services { get; }
}

public static class FeatureManagementClientExtensions
{
    /// <summary>
    /// Регистрирует HTTP-клиент каталога + hosted-сервис старта (регистрация флагов + первый pull реплики).
    /// </summary>
    public static IFeatureManagementClientBuilder AddFeatureManagementClient(
        this IServiceCollection services, Action<FeatureManagementClientOptions> configure)
    {
        var options = new FeatureManagementClientOptions();
        configure(options);

        services.AddHttpClient<IFeatureCatalogClient, HttpFeatureCatalogClient>(c =>
            c.BaseAddress = new Uri(options.BaseUrl));

        services.AddHostedService<FeatureClientHostedService>();
        return new Builder(services);
    }

    /// <summary>
    /// Включает локальную in-memory реплику определений (<see cref="RemoteFeatureDefinitionProvider"/>)
    /// как реализацию порта — для микросервиса-потребителя (горячий путь без сети). Подписку на события
    /// инвалидации выполняет модуль клиента в <c>OnApplicationInitialization</c>.
    /// </summary>
    public static IFeatureManagementClientBuilder UseRemoteReplica(this IFeatureManagementClientBuilder builder)
    {
        builder.Services.TryAddSingleton<RemoteFeatureDefinitionProvider>();
        builder.Services.AddSingleton<IFeatureDefinitionProvider>(
            sp => sp.GetRequiredService<RemoteFeatureDefinitionProvider>());
        return builder;
    }

    /// <summary>Декларирует флаги потребителя (синкаются в каталог при старте).</summary>
    public static IFeatureManagementClientBuilder RegisterFeatures(
        this IFeatureManagementClientBuilder builder, Action<IFeatureRegistrationBuilder> configure)
    {
        var reg = new RegistrationBuilder();
        configure(reg);
        builder.Services.AddSingleton(new FeatureRegistrationContribution(reg.Descriptors));
        return builder;
    }

    private sealed class Builder(IServiceCollection services) : IFeatureManagementClientBuilder
    {
        public IServiceCollection Services { get; } = services;
    }

    private sealed class RegistrationBuilder : IFeatureRegistrationBuilder
    {
        public List<FeatureDefinitionDescriptor> Descriptors { get; } = new();

        public IFeatureRegistrationBuilder Add(string key, string name, Action<FeatureDescriptorOptions>? configure = null)
        {
            var o = new FeatureDescriptorOptions();
            configure?.Invoke(o);
            var ownerService = key.Contains('.') ? key[..key.IndexOf('.')] : key;
            Descriptors.Add(new FeatureDefinitionDescriptor(key, name, ownerService, o.ValueType, o.Description, o.Variants));
            return this;
        }
    }
}
