using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Core.Modules;

/// <summary>
/// Base interface for all CRM modules
/// </summary>
public interface ICrmModule
{
    /// <summary>
    /// Unique name of the module
    /// </summary>
    string? Name { get; }
    
    /// <summary>
    /// Called before ConfigureServices - for early initialization
    /// </summary>
    void PreConfigure() { }
    
    /// <summary>
    /// Configure services for dependency injection
    /// </summary>
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);
    
    /// <summary>
    /// Called after ConfigureServices - for late configuration
    /// </summary>
    void PostConfigure() { }
    
    /// <summary>
    /// Called before module initialization
    /// </summary>
    ValueTask OnPreInitializeAsync(IServiceProvider services) => ValueTask.CompletedTask;
    
    /// <summary>
    /// Main module initialization (e.g., run migrations)
    /// </summary>
    ValueTask OnInitializeAsync(IServiceProvider services) => ValueTask.CompletedTask;
    
    /// <summary>
    /// Called after module initialization
    /// </summary>
    ValueTask OnPostInitializeAsync(IServiceProvider services) => ValueTask.CompletedTask;
}