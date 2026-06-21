using Cheetah.Workflow;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.Workflow.Client;

/// <summary>Опции HTTP-клиента каталога Workflow.</summary>
public sealed class WorkflowClientOptions
{
    public string BaseUrl { get; set; } = "http://localhost";
}

/// <summary>Вклад одного контрибутора в реестр Workflow (агрегируется hosted-сервисом при старте).</summary>
public sealed record WorkflowRegistrationContribution(
    IReadOnlyList<TriggerDescriptor> Triggers,
    IReadOnlyList<ActionDescriptor> Actions);

public interface IWorkflowTriggerRegistrationBuilder
{
    IWorkflowTriggerRegistrationBuilder Add(string eventName, string ownerService, string title,
        IReadOnlyList<string>? payload = null);
}

public interface IWorkflowActionRegistrationBuilder
{
    IWorkflowActionRegistrationBuilder Add(string name, string ownerService, string title,
        ActionTransport transport = ActionTransport.InProc, Action<IActionParamsBuilder>? parameters = null);
}

public interface IActionParamsBuilder
{
    IActionParamsBuilder Param(string key, string type, bool required = false, string? @default = null);
}

public interface IWorkflowClientBuilder
{
    IServiceCollection Services { get; }
}

public static class WorkflowClientExtensions
{
    /// <summary>Регистрирует HTTP-клиент каталога Workflow + hosted-сервис регистрации триггеров/действий при старте.</summary>
    public static IWorkflowClientBuilder AddWorkflowClient(
        this IServiceCollection services, Action<WorkflowClientOptions> configure)
    {
        var options = new WorkflowClientOptions();
        configure(options);

        services.AddHttpClient<IWorkflowCatalogClient, HttpWorkflowCatalogClient>(c =>
            c.BaseAddress = new Uri(options.BaseUrl));

        services.AddHostedService<WorkflowRegistrationSyncService>();
        return new Builder(services);
    }

    /// <summary>Декларирует триггеры контрибутора (события, на которые можно строить правила).</summary>
    public static IWorkflowClientBuilder RegisterTriggers(
        this IWorkflowClientBuilder builder, Action<IWorkflowTriggerRegistrationBuilder> configure)
    {
        var reg = new TriggerBuilder();
        configure(reg);
        builder.Services.AddSingleton(new WorkflowRegistrationContribution(reg.Triggers, Array.Empty<ActionDescriptor>()));
        return builder;
    }

    /// <summary>Декларирует действия контрибутора (операции, доступные правилам).</summary>
    public static IWorkflowClientBuilder RegisterActions(
        this IWorkflowClientBuilder builder, Action<IWorkflowActionRegistrationBuilder> configure)
    {
        var reg = new ActionBuilder();
        configure(reg);
        builder.Services.AddSingleton(new WorkflowRegistrationContribution(Array.Empty<TriggerDescriptor>(), reg.Actions));
        return builder;
    }

    /// <summary>Декларирует готовый список триггеров (удобно для адаптеров, экспонирующих свои дескрипторы).</summary>
    public static IWorkflowClientBuilder RegisterTriggers(
        this IWorkflowClientBuilder builder, IReadOnlyList<TriggerDescriptor> triggers)
    {
        builder.Services.AddSingleton(new WorkflowRegistrationContribution(triggers, Array.Empty<ActionDescriptor>()));
        return builder;
    }

    /// <summary>Декларирует готовый список действий (удобно для адаптеров, экспонирующих свои дескрипторы).</summary>
    public static IWorkflowClientBuilder RegisterActions(
        this IWorkflowClientBuilder builder, IReadOnlyList<ActionDescriptor> actions)
    {
        builder.Services.AddSingleton(new WorkflowRegistrationContribution(Array.Empty<TriggerDescriptor>(), actions));
        return builder;
    }

    private sealed class Builder(IServiceCollection services) : IWorkflowClientBuilder
    {
        public IServiceCollection Services { get; } = services;
    }

    private sealed class TriggerBuilder : IWorkflowTriggerRegistrationBuilder
    {
        public List<TriggerDescriptor> Triggers { get; } = new();

        public IWorkflowTriggerRegistrationBuilder Add(string eventName, string ownerService, string title,
            IReadOnlyList<string>? payload = null)
        {
            Triggers.Add(new TriggerDescriptor(eventName, ownerService, title, payload ?? Array.Empty<string>()));
            return this;
        }
    }

    private sealed class ActionBuilder : IWorkflowActionRegistrationBuilder
    {
        public List<ActionDescriptor> Actions { get; } = new();

        public IWorkflowActionRegistrationBuilder Add(string name, string ownerService, string title,
            ActionTransport transport = ActionTransport.InProc, Action<IActionParamsBuilder>? parameters = null)
        {
            var pb = new ParamsBuilder();
            parameters?.Invoke(pb);
            Actions.Add(new ActionDescriptor(name, ownerService, title, pb.Parameters, transport));
            return this;
        }
    }

    private sealed class ParamsBuilder : IActionParamsBuilder
    {
        public List<ActionParameterDescriptor> Parameters { get; } = new();

        public IActionParamsBuilder Param(string key, string type, bool required = false, string? @default = null)
        {
            Parameters.Add(new ActionParameterDescriptor(key, type, required, @default));
            return this;
        }
    }
}
