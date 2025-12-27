using Cheetah.Core;
using Cheetah.Core.DependencyInjection;

namespace Cheetah.AspNetCore.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder InitializeApplication(this IApplicationBuilder app)
    {
        if (app == null) throw new ArgumentNullException(nameof(app));
        app.ApplicationServices.GetRequiredService<ObjectAccessor<IApplicationBuilder>>().Value = app;
        app.ApplicationServices.GetRequiredService<ObjectAccessor<IEndpointRouteBuilder>>().Value = app as IEndpointRouteBuilder;
        var application = app.ApplicationServices.GetRequiredService<ICrmApplicationWithExternalServiceProvider>();
        var applicationLifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();

        applicationLifetime.ApplicationStopped.Register(() =>
        {
            application.Dispose();
        });

        application.Initialize(app.ApplicationServices);

        return app;
    }

}