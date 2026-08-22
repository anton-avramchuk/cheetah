using Cheetah.Core;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Notes.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cheetah.Modules.Notes.Client;

public sealed class NotesClientOptions
{
    /// <summary>Базовый URL Notes.Api. Обязателен.</summary>
    public string BaseUrl { get; set; } = "";

    /// <summary>Тайм-аут HTTP-запросов. Default = 5 секунд.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);
}

internal sealed class NotesClientOptionsValidator : IValidateOptions<NotesClientOptions>
{
    public ValidateOptionsResult Validate(string? name, NotesClientOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.BaseUrl))
            return ValidateOptionsResult.Fail("Notes:Client:BaseUrl is required");
        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _))
            return ValidateOptionsResult.Fail("Notes:Client:BaseUrl must be a valid absolute URI");
        if (options.Timeout <= TimeSpan.Zero)
            return ValidateOptionsResult.Fail("Notes:Client:Timeout must be positive");
        return ValidateOptionsResult.Success;
    }
}

public static class NotesClientServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует типизированный HTTP-клиент заметок для закрытых типов приложения. Открытые
    /// generic-типы source-генератору <c>[Export]</c> недоступны, поэтому регистрация ручная —
    /// вызывается наследником после подключения <see cref="CheetahNotesClientModule"/>.
    /// </summary>
    public static IServiceCollection AddNotesClient<TCreateRequest, TUpdateRequest, TDto>(
        this IServiceCollection services)
        where TCreateRequest : CreateNoteRequestBase
        where TUpdateRequest : UpdateNoteRequestBase
        where TDto : NoteDtoBase
    {
        services.AddHttpClient<
            INotesClient<TCreateRequest, TUpdateRequest, TDto>,
            HttpNotesClient<TCreateRequest, TUpdateRequest, TDto>>((sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<NotesClientOptions>>().Value;
            client.BaseAddress = new Uri(opts.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = opts.Timeout;
        });

        return services;
    }
}

/// <summary>
/// Подключает HTTP-клиент к Notes.Api: биндит секцию <c>Notes:Client</c> и валидирует опции на старте.
/// Сам клиент регистрируется закрытыми типами через <c>AddNotesClient&lt;…&gt;()</c>.
/// </summary>
[DependsOn(typeof(CoreModule), typeof(CheetahNotesContractsModule))]
public partial class CheetahNotesClientModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        var configuration = services.GetConfiguration();

        services.AddOptions<NotesClientOptions>()
            .Bind(configuration.GetSection("Notes:Client"))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<NotesClientOptions>, NotesClientOptionsValidator>();

        RegisterServices(services);
    }
}
