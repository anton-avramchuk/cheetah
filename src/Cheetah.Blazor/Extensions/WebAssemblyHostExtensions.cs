using Cheetah.Core;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Blazor.Extensions;

public static class WebAssemblyHostExtensions
{
    public static WebAssemblyHost InitializeApplication(this WebAssemblyHost host)
    {
        if (host == null) throw new ArgumentNullException(nameof(host));

        var application = host.Services.GetRequiredService<ICrmApplicationWithExternalServiceProvider>();
        application.Initialize(host.Services);

        return host;
    }
}
