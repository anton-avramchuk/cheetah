using Cheetah.Core.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace Cheetah.Modules.Identity.Application.Exceptions;

/// <summary>
/// Неуспешный <see cref="IdentityResult"/> в виде исключения: описания ошибок ASP.NET Identity
/// склеиваются в сообщение, которое обработчик отдаёт клиенту как 400.
/// <para>
/// Живёт в Application, а не в Infrastructure: тип бросают обработчики команд, и хранить его
/// в EF-сборке значило бы тянуть Application к инфраструктуре ради одного <c>using</c>.
/// </para>
/// </summary>
public class IdentityException : CrmException
{
    public IdentityException(IdentityResult result)
        : base(string.Join("\n", (result ?? throw new ArgumentNullException(nameof(result))).Errors.Select(x => x.Description)))
    {
    }
}
