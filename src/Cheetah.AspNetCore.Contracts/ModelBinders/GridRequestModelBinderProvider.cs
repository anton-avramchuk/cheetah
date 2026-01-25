using Cheetah.AspNetCore.Contracts.Requests;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Cheetah.AspNetCore.Contracts.ModelBinders
{
    /// <summary>
    /// Provider для регистрации GridRequestModelBinder в ASP.NET Core
    /// </summary>
    public class GridRequestModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.Metadata.ModelType == typeof(GridRequest))
            {
                return new GridRequestModelBinder();
            }

            return null;
        }
    }
}
