namespace Cheetah.Core.ExceptionHandling;

public interface IHasHttpStatusCode
{
    int HttpStatusCode { get; }
}