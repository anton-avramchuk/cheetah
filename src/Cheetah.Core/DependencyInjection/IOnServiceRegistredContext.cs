using Cheetah.Core.Collections;
using Cheetah.Core.Common;

namespace Cheetah.Core.DependencyInjection;

public interface IOnServiceRegistredContext
{
    ITypeList<ICrmInterceptor> Interceptors { get; }
    Type ImplementationType { get; }
}