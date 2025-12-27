using Cheetah.Core.DependencyInjection;

namespace Cheetah.Core.ExceptionHandling;

[Export(LifetimeType.Transient, typeof(IExceptionSubscriber))]
public abstract class ExceptionSubscriber : IExceptionSubscriber
{
    public abstract Task HandleAsync(ExceptionNotificationContext context);
}