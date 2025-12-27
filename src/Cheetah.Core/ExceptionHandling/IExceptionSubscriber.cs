namespace Cheetah.Core.ExceptionHandling;

public interface IExceptionSubscriber
{
    Task HandleAsync(ExceptionNotificationContext context);
}