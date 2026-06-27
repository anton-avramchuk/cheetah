using Microsoft.AspNetCore.Routing;

namespace Cheetah.Modules.Identity.Api.Registration;

/// <summary>
/// Замыкание регистрации эндпоинтов Identity, захваченное builder-ом со всеми
/// конкретными типами хоста. Вызывается модулем Identity.Api в фазе
/// OnApplicationInitialization (когда уже доступен RouteBuilder).
/// </summary>
public interface IIdentityEndpointRegistrar
{
    void Map(IEndpointRouteBuilder routes);
}

internal sealed class DelegateEndpointRegistrar(Action<IEndpointRouteBuilder> map) : IIdentityEndpointRegistrar
{
    public void Map(IEndpointRouteBuilder routes) => map(routes);
}
