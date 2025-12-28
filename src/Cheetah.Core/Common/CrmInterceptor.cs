namespace Cheetah.Core.Common;

public abstract class CrmInterceptor : ICrmInterceptor
{
    public abstract Task InterceptAsync(ICrmMethodInvocation invocation);
}