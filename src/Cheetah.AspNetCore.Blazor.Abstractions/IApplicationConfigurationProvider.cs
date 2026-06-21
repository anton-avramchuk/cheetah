namespace Cheetah.AspNetCore.Blazor.Abstractions;

public interface IApplicationConfigurationProvider
{
    ApplicationLogo Logo { get; }
    ApplicationTitle Title { get; }
}
