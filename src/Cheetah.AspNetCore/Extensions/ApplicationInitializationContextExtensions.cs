using Cheetah.Core;
using Cheetah.Core.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cheetah.AspNetCore.Extensions;

public static class ApplicationInitializationContextExtensions
{
    extension(ApplicationInitializationContext context)
    {
        public IApplicationBuilder GetApplicationBuilder()
        {
            return context.ServiceProvider.GetRequiredService<IObjectAccessor<IApplicationBuilder>>().Value!;
        }

        public IEndpointRouteBuilder GetRouteBuilder()
        {
            return context.ServiceProvider.GetRequiredService<IObjectAccessor<IEndpointRouteBuilder>>().Value!;
        }

        public IWebHostEnvironment GetEnvironment()
        {
            return context.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        }

        public IWebHostEnvironment GetEnvironmentOrNull()
        {
            return context.ServiceProvider.GetService<IWebHostEnvironment>()!;
        }

        public IConfiguration GetConfiguration()
        {
            return context.ServiceProvider.GetRequiredService<IConfiguration>();
        }

        public ILoggerFactory GetLoggerFactory()
        {
            return context.ServiceProvider.GetRequiredService<ILoggerFactory>();
        }

        public TOptions GetOptions<TOptions>()
            where TOptions : class
        {
            return context.ServiceProvider.GetRequiredService<IOptions<TOptions>>().Value;
        }
    }
}