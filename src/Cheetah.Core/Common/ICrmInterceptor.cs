namespace Cheetah.Core.Common;

public interface ICrmInterceptor
{
    Task InterceptAsync(ICrmMethodInvocation invocation);
}