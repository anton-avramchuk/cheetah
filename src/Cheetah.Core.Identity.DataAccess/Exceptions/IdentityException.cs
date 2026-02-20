using Cheetah.Core.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace Cheetah.Core.Identity.DataAccess.Exceptions;

public class IdentityException : CrmException
{
    public IdentityException(IdentityResult result)
        : base(string.Join("\n", (result ?? throw new ArgumentNullException(nameof(result))).Errors.Select(x => x.Description)))
    {
    }
}
